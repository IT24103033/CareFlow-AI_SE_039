using System;

namespace CareFlowAI.API.Models
{
    public class TriageReviewHistory
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public Guid TriageRecordId { get; set; }
        public TriageRecord TriageRecord { get; set; } = null!;

        public Guid? AssignedDoctorId { get; set; }
        public string Action { get; set; } = string.Empty; // "Approved", "Rejected", "RevisionRequested"
        public string? Notes { get; set; }
        
        public string SymptomsAtReview { get; set; } = string.Empty;
        
        public Guid? AgentWorkflowStateId { get; set; }
        public AgentWorkflowState? AgentWorkflowState { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
