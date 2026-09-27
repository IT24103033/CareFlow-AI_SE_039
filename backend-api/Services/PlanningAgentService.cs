using System.Diagnostics;
using System.Text.Json;
using CareFlowAI.API.Models;

namespace CareFlowAI.API.Services;

/// <summary>
/// Component B planner. Owns assessment and a controlled delegation plan, not
/// scheduling, safety-agent execution, or appointment confirmation.
/// </summary>
public class PlanningAgentService(IPatientContextTool contextTool, IClinicalAssessmentClient assessmentClient,
    IConfiguration configuration, ILogger<PlanningAgentService>? logger = null)
{
        private static readonly List<(string[] Keywords, string Specialist, string Urgency, string Action)> _criticalRules = new()
        {
            (new[] { "chest pain", "chest tightness", "heart attack", "palpitation" },
             "Cardiologist",  "Critical", "Send to Emergency Department immediately."),

            (new[] { "shortness of breath", "cannot breathe", "difficulty breathing" },
             "Pulmonologist", "Critical", "Send to Emergency Department immediately."),

            (new[] { "stroke", "face drooping", "sudden numbness", "arm weakness", "speech difficulty" },
             "Neurologist",   "Critical", "Send to Emergency Department immediately."),

            (new[] { "seizure", "unconscious", "unresponsive", "not breathing" },
             "Neurologist",   "Critical", "Send to Emergency Department immediately."),

            (new[] { "severe bleeding", "blood loss", "hemorrhage" },
             "General Surgeon","Critical", "Send to Emergency Department immediately."),

            (new[] { "severe headache", "migraine", "sudden head pain" },
             "Neurologist",   "High",     "Schedule urgent appointment within 24 hours."),

            (new[] { "burning urination", "blood in urine", "kidney pain" },
             "Urologist",     "High",     "Schedule urgent appointment within 24 hours."),
        };


    public static AgentWorkflowState CreateRunningState(TriageRecord record, string? duration)
    {
        var state = new AgentWorkflowState
        {
            TriageRecordId = record.Id, AgentName = "PlanningAgent", AgentStatus = "Running",
            ApprovalStatus = "Pending", StartedAt = DateTime.UtcNow
        };
        state.InputPayload = JsonSerializer.Serialize(new PlanningInput(state.Id, record.PatientId,
            PlanningPlanValidator.Objective, record.Symptoms, duration?.Trim()));
        state.OutputPayload = JsonSerializer.Serialize(new ClinicalPlan { SchemaVersion = 2,
            Execution = new PlanningExecutionSummary() });
        return state;
    }

    public async Task RunAsync(TriageRecord record, AgentWorkflowState state, CancellationToken cancellationToken = default)
    {
        if (state.TriageRecordId != record.Id || state.AgentStatus != "Running")
            throw new ArgumentException("Only the matching running workflow can be executed.");
        var trace = new PlanningExecutionSummary();
        ClinicalPlan? plan = null;
        string? failure = null;
        var maxAttempts = Math.Clamp(configuration.GetValue("Planning:MaxAttempts", 2), 1, 3);
        var attemptTimeout = TimeSpan.FromSeconds(Math.Clamp(configuration.GetValue("Planning:AttemptTimeoutSeconds", 10), 1, 30));
        using var budget = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        budget.CancelAfter(TimeSpan.FromSeconds(Math.Clamp(configuration.GetValue("Planning:WorkflowTimeoutSeconds", 30), 1, 90)));
        try
        {
            var input = JsonSerializer.Deserialize<PlanningInput>(state.InputPayload);
            if (input == null || input.PatientId != record.PatientId || input.WorkflowId != state.Id ||
                input.Objective != PlanningPlanValidator.Objective || input.Symptoms != record.Symptoms ||
                string.IsNullOrWhiteSpace(input.Symptoms) || input.Symptoms.Trim().Length < 20 || input.Symptoms.Length > 4000 ||
                input.Duration?.Length > 200)
            {
                failure = "INVALID_INPUT";
            }
            else
            {
                budget.Token.ThrowIfCancellationRequested();
                var timer = Stopwatch.StartNew();
                var context = await contextTool.GetPatientProfileAsync(input.PatientId, budget.Token).WaitAsync(budget.Token);
                timer.Stop();
                if (context == null || context.PatientId != input.PatientId || context.MedicalHistorySummary == null ||
                    context.MedicalHistorySummary.Length > 4000)
                {
                    trace.Events.Add(new("GetPatientProfile", "InvalidContext", timer.ElapsedMilliseconds));
                    failure = "INVALID_PATIENT_CONTEXT";
                }
                else
                {
                    trace.Events.Add(new("GetPatientProfile", "Completed", timer.ElapsedMilliseconds));
                    state.InputPayload = JsonSerializer.Serialize(input with { Context = context });
                    plan = MatchExistingKeywordRules(input.Symptoms);
                    if (plan != null) trace.Events.Add(new("KeywordScreen", "Matched", 0));
                    else
                    {
                        var assessmentInput = new ClinicalAssessmentInput(input.Symptoms, input.Duration, context.MedicalHistorySummary);
                        for (var attempt = 1; attempt <= maxAttempts && plan == null; attempt++)
                        {
                            budget.Token.ThrowIfCancellationRequested();
                            trace.ModelAttempts = attempt;
                            using var deadline = CancellationTokenSource.CreateLinkedTokenSource(budget.Token);
                            deadline.CancelAfter(attemptTimeout);
                            timer.Restart();
                            try
                            {
                                var response = await assessmentClient.AssessAsync(assessmentInput, deadline.Token).WaitAsync(deadline.Token);
                                plan = PlanningPlanValidator.ParseAssessment(response);
                                failure = plan == null ? "INVALID_MODEL_OUTPUT" : null;
                                trace.Events.Add(new("AssessSymptoms", plan == null ? "InvalidOutput" : "Completed", timer.ElapsedMilliseconds));
                                if (plan != null) plan.AnalysisMethod = "GeminiAI";
                            }
                            catch (AssessmentConfigurationException)
                            {
                                failure = "PROVIDER_NOT_CONFIGURED";
                                trace.Events.Add(new("AssessSymptoms", "NotConfigured", timer.ElapsedMilliseconds));
                                break;
                            }
                            catch (OperationCanceledException) when (!budget.IsCancellationRequested)
                            {
                                failure = "PROVIDER_TIMEOUT";
                                trace.Events.Add(new("AssessSymptoms", "TimedOut", timer.ElapsedMilliseconds));
                            }
                            catch (OperationCanceledException) { throw; }
                            catch (Exception exception)
                            {
                                // Provider exceptions may contain request bodies or credentials.
                                logger?.LogWarning("Planning workflow {WorkflowId} provider attempt {Attempt} failed with exception type {ExceptionType}",
                                    state.Id, attempt, exception.GetType().Name);
                                failure = "PROVIDER_UNAVAILABLE";
                                trace.Events.Add(new("AssessSymptoms", "Unavailable", timer.ElapsedMilliseconds));
                            }
                        }
                    }
                    if (plan != null)
                    {
                        budget.Token.ThrowIfCancellationRequested();
                        plan.SchemaVersion = 2;
                        plan.Objective = PlanningPlanValidator.Objective;
                        plan.Steps = PlanningPlanValidator.BuildSteps(plan.UrgencyLevel);
                        if (string.IsNullOrWhiteSpace(context.MedicalHistorySummary))
                            plan.Warnings.Add("Patient medical history is not recorded; confirm it during review.");
                        if (plan.AnalysisMethod == "RuleEngine")
                            plan.Warnings.Add("Keyword screening is not a completed Safety Agent validation.");
                        trace.Status = "Completed";
                        plan.Execution = trace;
                        if (!PlanningPlanValidator.IsStructuredPlanValid(plan))
                        { failure = "INVALID_PLAN"; plan = null; }
                        else
                        { failure = null; trace.Events.Add(new("ValidatePlan", "Passed", 0)); }
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            failure = cancellationToken.IsCancellationRequested ? "CANCELLED" : "WORKFLOW_TIMEOUT";
            plan = null;
        }
        catch (Exception)
        {
            failure = "PLANNING_FAILED";
            plan = null;
        }
        if (plan == null)
        {
            trace.Status = "Failed";
            trace.FailureCode = failure ?? "PLANNING_FAILED";
            trace.Events.Add(new("PlanningAgent", trace.FailureCode, 0));
            plan = new ClinicalPlan { SchemaVersion = 2, Execution = trace };
            state.AgentStatus = "Failed";
            state.ErrorMessage = trace.FailureCode;
            logger?.LogWarning("Planning workflow {WorkflowId} failed: {FailureCode}; model attempts {ModelAttempts}",
                state.Id, trace.FailureCode, trace.ModelAttempts);
            foreach (var step in trace.Events)
                logger?.LogWarning("Planning workflow {WorkflowId}: {Operation} {Outcome} in {DurationMs} ms",
                    state.Id, step.Operation, step.Outcome, step.DurationMs);
        }
        else
        {
            state.AgentStatus = "Completed";
            state.ErrorMessage = null;
        }
        state.OutputPayload = JsonSerializer.Serialize(plan);
        state.CompletedAt = DateTime.UtcNow;
        state.UpdatedAt = state.CompletedAt.Value;
    }

    private static ClinicalPlan? MatchExistingKeywordRules(string symptoms)
    {
        var lower = symptoms.ToLowerInvariant();
        foreach (var (keywords, specialist, urgency, action) in _criticalRules)
        {
            if (keywords.Any(lower.Contains)) return new ClinicalPlan
            {
                SuggestedSpecialist = specialist, UrgencyLevel = urgency, RecommendedAction = action,
                Rationale = "The existing keyword screen detected: " + string.Join(", ", keywords.Where(lower.Contains)),
                AnalysisMethod = "RuleEngine"
            };
        }
        return null;
    }
}
