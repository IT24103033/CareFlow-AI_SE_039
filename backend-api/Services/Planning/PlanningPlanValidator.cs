using System.Text.Json;
using System.Text.Json.Serialization;

namespace CareFlowAI.API.Services;

public static class PlanningPlanValidator
{
    public const string Objective = "Prepare a triage assessment and a controlled plan for clinician review.";
    public static readonly string[] Specialties = { "Cardiologist", "Pulmonologist", "Neurologist", "General Surgeon",
        "Urologist", "Gastroenterologist", "Orthopedist", "Ophthalmologist", "Psychiatrist", "Dermatologist", "General Practitioner" };

    // The model has no authority to choose tools, grant approvals or declare steps complete.
    private sealed class ModelAssessment
    {
        public string SuggestedSpecialist { get; set; } = string.Empty;
        public string UrgencyLevel { get; set; } = string.Empty;
        public string RecommendedAction { get; set; } = string.Empty;
        public string Rationale { get; set; } = string.Empty;
    }

    public static ClinicalPlan? ParseAssessment(string response)
    {
        if (string.IsNullOrWhiteSpace(response) || response.Length > 16000) return null;
        var json = response.Trim();
        if (json.StartsWith("```json", StringComparison.Ordinal) && json.EndsWith("```", StringComparison.Ordinal))
            json = json[7..^3].Trim();
        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Object) return null;
            var properties = document.RootElement.EnumerateObject().ToArray();
            if (properties.Length != 4 || properties.Select(p => p.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count() != 4)
                return null;
            var result = JsonSerializer.Deserialize<ModelAssessment>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true, UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
            });
            if (result == null) return null;
            var plan = new ClinicalPlan { SuggestedSpecialist = result.SuggestedSpecialist,
                UrgencyLevel = result.UrgencyLevel, RecommendedAction = result.RecommendedAction, Rationale = result.Rationale };
            return IsAssessmentValid(plan) && Specialties.Contains(plan.SuggestedSpecialist) ? plan : null;
        }
        catch (JsonException) { return null; }
    }

    public static bool IsAssessmentValid(ClinicalPlan plan) =>
        new[] { "Low", "Medium", "High", "Critical" }.Contains(plan.UrgencyLevel) &&
        !string.IsNullOrWhiteSpace(plan.SuggestedSpecialist) && plan.SuggestedSpecialist.Length <= 120 &&
        !string.IsNullOrWhiteSpace(plan.RecommendedAction) && plan.RecommendedAction.Length <= 2000 &&
        !string.IsNullOrWhiteSpace(plan.Rationale) && plan.Rationale.Length <= 4000;

    public static List<PlanStep> BuildSteps(string urgency)
    {
        var steps = new List<PlanStep> { new("safety", "SafetyAgent", "CheckEmergencyRules", Array.Empty<string>()) };
        if (urgency == "Critical")
        {
            steps.Add(new("urgent-review", "HumanReviewer", "ReviewTriage", new[] { "safety" }, true));
            return steps; // Critical assessments never request routine appointment automation.
        }
        steps.AddRange(new[] {
            new PlanStep("find-slot", "ActionAgent", "GetAvailableSlots", new[] { "safety" }),
            new PlanStep("hold-slot", "ActionAgent", "CreateTentativeAppointment", new[] { "find-slot" }),
            new PlanStep("review", "HumanReviewer", "ReviewTriage", new[] { "hold-slot" }, true),
            new PlanStep("confirm", "ActionAgent", "ConfirmAppointment", new[] { "review" }, true),
            new PlanStep("notify", "NotificationService", "SendAppointmentNotification", new[] { "confirm" }, true)
        });
        return steps;
    }

    public static bool IsStructuredPlanValid(ClinicalPlan plan)
    {
        if (plan.SchemaVersion != 2 || !IsAssessmentValid(plan) || !Specialties.Contains(plan.SuggestedSpecialist) ||
            plan.Objective != Objective || plan.Execution?.Status != "Completed" || plan.Execution.FailureCode != null ||
            plan.Steps == null || plan.Warnings == null ||
            !new[] { "GeminiAI", "RuleEngine" }.Contains(plan.AnalysisMethod)) return false;
        var expected = BuildSteps(plan.UrgencyLevel);
        // Comparing against controlled routes also rejects cycles, injected tools, missing
        // approval gates and attempts to claim another agent has already executed a step.
        return plan.Steps.Count == expected.Count && plan.Steps.Zip(expected).All(pair =>
            pair.First != null && pair.First.Id == pair.Second.Id && pair.First.Agent == pair.Second.Agent &&
            pair.First.Tool == pair.Second.Tool && pair.First.Status == "Pending" &&
            pair.First.RequiresHumanApproval == pair.Second.RequiresHumanApproval &&
            pair.First.DependsOn != null && pair.First.DependsOn.SequenceEqual(pair.Second.DependsOn));
    }
}
