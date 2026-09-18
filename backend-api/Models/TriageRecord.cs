using System;
using System.Collections.Generic;

namespace CareFlowAI.API.Models
{
    /// <summary>
    /// Stores a patient's symptom submission and tracks the AI triage lifecycle.
    /// FK: PatientId → PatientProfiles.Id (owned by Component A).
    /// </summary>
    public class TriageRecord
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // ── Foreign Key to Component A's PatientProfiles table ──────────────
        public Guid PatientId { get; set; }
        public PatientProfile Patient { get; set; } = null!;

        // ── Clinical Input ───────────────────────────────────────────────────
        /// <summary>Raw symptom text submitted by the patient from the Flutter app.</summary>
        public string Symptoms { get; set; } = string.Empty;

        /// <summary>
        /// AI-assessed urgency: "Low" | "Medium" | "High" | "Critical"
        /// </summary>
        public string SeverityLevel { get; set; } = "Medium";

        // ── Triage Lifecycle ─────────────────────────────────────────────────
        /// <summary>
        /// Workflow state: "Pending" | "InReview" | "Approved" | "Rejected"
        /// </summary>
        public string TriageStatus { get; set; } = "Pending";

        // ── Doctor Review Fields (populated after human approval) ────────────
        /// <summary>Doctor's notes written during the React dashboard review.</summary>
        public string? DoctorNotes { get; set; }

        /// <summary>
        /// Id of the doctor who approved/rejected. Populated by Component C (Scheduling).
        /// </summary>
        public Guid? AssignedDoctorId { get; set; }

        // ── Audit Fields (required by normalized schema) ─────────────────────
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // ── Navigation Property ───────────────────────────────────────────────
        /// <summary>All AI agent runs triggered by this triage submission.</summary>
        public ICollection<AgentWorkflowState> AgentWorkflows { get; set; } = new List<AgentWorkflowState>();
    }
}
