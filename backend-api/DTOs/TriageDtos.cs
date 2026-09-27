namespace CareFlowAI.API.DTOs
{
    /// <summary>
    /// Payload the Flutter app sends when a patient submits symptoms.
    /// </summary>
    public class CreateTriageRequestDto
    {
        public Guid PatientId { get; set; }
        public string Symptoms { get; set; } = string.Empty;
    }

    /// <summary>
    /// Payload the React dashboard sends when a doctor approves or rejects a triage plan.
    /// </summary>
    public class ReviewTriageDto
    {
        /// <summary>"Approved" or "Rejected"</summary>
        public string Decision { get; set; } = string.Empty;
        public string? DoctorNotes { get; set; }
        public Guid? AssignedDoctorId { get; set; }
    }

    /// <summary>
    /// Safe read-only response returned to clients — no internal IDs leaked.
    /// </summary>
    public class TriageResponseDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string Symptoms { get; set; } = string.Empty;
        public string SeverityLevel { get; set; } = string.Empty;
        public string TriageStatus { get; set; } = string.Empty;
        public string? DoctorNotes { get; set; }
        public Guid? AssignedDoctorId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Summary of AI agent run attached to this record
        public string? AiPlan           { get; set; }
        public string? AiAgentStatus    { get; set; }
        public string? ApprovalStatus   { get; set; }
        /// <summary>Which analysis path was used: RuleEngine | GeminiAI | FallbackRules</summary>
        public string? AnalysisMethod   { get; set; }
    }
}
