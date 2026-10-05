using System.ComponentModel.DataAnnotations;
using CareFlowAI.API.Services;

namespace CareFlowAI.API.DTOs
{
    /// <summary>
    /// Payload the Flutter app sends when a patient submits symptoms.
    /// </summary>
    public class CreateTriageRequestDto
    {
        public Guid PatientId { get; set; }
        [Required, StringLength(4000, MinimumLength = 20)]
        public string Symptoms { get; set; } = string.Empty;
        [StringLength(200)]
        public string? Duration { get; set; }
        public Guid? AttachmentId { get; set; }
    }

    /// <summary>
    /// Payload the Flutter app sends when a patient submits a revision.
    /// </summary>
    public class PatientRevisionDto
    {
        [Required, StringLength(4000, MinimumLength = 20)]
        public string UpdatedSymptoms { get; set; } = string.Empty;
        
        [Required]
        public DateTime? ExpectedUpdatedAt { get; set; }
    }

    /// <summary>
    /// Payload the React dashboard sends when a doctor approves or rejects a triage plan.
    /// </summary>
    public class ReviewTriageDto
    {
        /// <summary>Approved, Rejected or RevisionRequested.</summary>
        [Required, RegularExpression("^(Approved|Rejected|RevisionRequested)$")]
        public string Decision { get; set; } = string.Empty;
        [StringLength(2000)]
        public string? DoctorNotes { get; set; }
        // Reviewer identity is derived from the authenticated doctor_id claim.
        [Required]
        public DateTime? ExpectedUpdatedAt { get; set; }
    }

    /// <summary>
    /// Triage response shared by clients; staff endpoints additionally populate PatientName.
    /// </summary>
    public class TriageResponseDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string? PatientName { get; set; }
        public string Symptoms { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
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
        public PlanningExecutionSummary? PlanningExecution { get; set; }
        public string? AnalysisMethod   { get; set; }
        
        // Component C (Scheduling)
        public Guid? TentativeAppointmentId { get; set; }
        public string? SchedulingOutcome { get; set; }
        public AppointmentDto? AppointmentDetails { get; set; }

        // Component D (Safety & Context)
        public string? SafetyVerdict { get; set; }
        public string? SafetySummary { get; set; }

        // Execution/Notification
        public string? NotificationOutcome { get; set; }

        public List<TriageReviewHistoryDto> ReviewHistories { get; set; } = new();
    }

    public class TriageReviewHistoryDto
    {
        public Guid Id { get; set; }
        public Guid? ReviewerId { get; set; }
        public Guid? LinkedAttemptId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string SymptomsAtReview { get; set; } = string.Empty;
        public string? PreviousPlan { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

namespace CareFlowAI.API.DTOs
{
    public class TriageQueueQuery
    {
        [StringLength(100)] public string? Search { get; set; }
        [RegularExpression("^(Pending|InReview|Approved|Rejected|RevisionRequested|AssessmentFailed|ReassessmentInProgress)$")]
        public string? Status { get; set; }
        [RegularExpression("^(Low|Medium|High|Critical|Unassessed)$")] public string? Severity { get; set; }
        [RegularExpression("^(newest|oldest|urgency)$")] public string Sort { get; set; } = "newest";
        [Range(1, 100000)] public int Page { get; set; } = 1;
        [Range(1, 100)] public int PageSize { get; set; } = 10;
    }
}
