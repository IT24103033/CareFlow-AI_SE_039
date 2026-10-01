namespace CareFlowAI.API.Services;

// Only the relevant profile summary is exposed to Component B. Component A can
// replace the adapter with its Context Agent without changing the planner.
public record PatientContextSnapshot(Guid PatientId, string MedicalHistorySummary, string BloodGroup, DateOnly DateOfBirth);
public record ClinicalAssessmentInput(string Symptoms, string? Duration, string MedicalHistorySummary, string BloodGroup, DateOnly DateOfBirth, string DomainRiskLevel, string[] DomainFlaggedFactors);
public record PlanningInput(Guid WorkflowId, Guid PatientId, string Objective, string Symptoms,
    string? Duration, PatientContextSnapshot? Context = null, CareFlowAI.Orchestrator.Agents.AgentOutput? DomainAnalysis = null);

public interface IPatientContextTool
{
    Task<PatientContextSnapshot?> GetPatientProfileAsync(Guid patientId, CancellationToken cancellationToken);
}

public interface IClinicalAssessmentClient
{
    Task<string> AssessAsync(ClinicalAssessmentInput input, CancellationToken cancellationToken);
}

public class AssessmentConfigurationException : Exception
{
    public AssessmentConfigurationException() : base("The assessment provider is not configured.") { }
}

public record PlanningEvent(string Operation, string Outcome, long DurationMs);
public class PlanningExecutionSummary
{
    public string Status { get; set; } = "Running";
    public string? FailureCode { get; set; }
    public int ModelAttempts { get; set; }
    public List<PlanningEvent> Events { get; set; } = new();
}

public record PlanStep(string Id, string Agent, string Tool, string[] DependsOn,
    bool RequiresHumanApproval = false, string Status = "Pending");

public class ClinicalPlan
{
    // Default 1 preserves existing persisted assessment-only payloads.
    public int SchemaVersion { get; set; } = 1;
    public string Objective { get; set; } = string.Empty;
    public string SuggestedSpecialist { get; set; } = string.Empty;
    public string UrgencyLevel { get; set; } = string.Empty;
    public string RecommendedAction { get; set; } = string.Empty;
    public string Rationale { get; set; } = string.Empty;
    public string AnalysisMethod { get; set; } = string.Empty;
    public List<PlanStep> Steps { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public PlanningExecutionSummary? Execution { get; set; }
}
