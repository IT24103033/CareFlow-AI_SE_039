using System;

namespace CareFlowAI.API.Models
{
    /// <summary>
    /// Tracks the execution state of each AI agent run for a specific triage record.
    /// One TriageRecord → many AgentWorkflowState rows (one per agent, e.g. PlanningAgent).
    /// </summary>
    public class AgentWorkflowState
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // ── Foreign Key to TriageRecord ──────────────────────────────────────
        public Guid TriageRecordId { get; set; }
        public TriageRecord TriageRecord { get; set; } = null!;

        // ── Agent Identity ───────────────────────────────────────────────────
        /// <summary>
        /// Name of the agent that ran. e.g. "PlanningAgent", "ValidationAgent".
        /// </summary>
        public string AgentName { get; set; } = string.Empty;

        /// <summary>
        /// Execution state: "Idle" | "Running" | "Completed" | "Failed"
        /// </summary>
        public string AgentStatus { get; set; } = "Idle";

        // ── Approval State (human-in-the-loop) ───────────────────────────────
        /// <summary>
        /// Doctor's decision on the AI plan: "Pending" | "Approved" | "Rejected"
        /// </summary>
        public string ApprovalStatus { get; set; } = "Pending";

        // ── AI Input / Output Payloads ────────────────────────────────────────
        /// <summary>JSON of the symptom data and context fed into the agent.</summary>
        public string InputPayload { get; set; } = string.Empty;

        /// <summary>
        /// JSON of the structured clinical plan the agent produced.
        /// e.g. { "SuggestedSpecialist": "Cardiologist", "UrgencyLevel": "High", ... }
        /// Null if the agent has not yet completed.
        /// </summary>
        public string? OutputPayload { get; set; }

        /// <summary>Populated when AgentStatus = "Failed".</summary>
        public string? ErrorMessage { get; set; }

        // ── Timing ────────────────────────────────────────────────────────────
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        // ── Audit Fields (required by normalized schema) ──────────────────────
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
