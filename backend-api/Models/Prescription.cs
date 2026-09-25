using System;
using System.Collections.Generic;

namespace CareFlowAI.API.Models
{
    /// <summary>
    /// Represents an e-prescription issued by a doctor after AI-validated triage.
    /// Component D: Pharmacy Inventory &amp; E-Prescriptions (Amodhya).
    /// </summary>
    public class Prescription
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // ── Foreign Keys ─────────────────────────────────────────────────────
        /// <summary>FK → PatientProfiles (Component A).</summary>
        public Guid PatientId { get; set; }
        public PatientProfile Patient { get; set; } = null!;

        /// <summary>FK → TriageRecords (Component B). Links prescription to the triage event.</summary>
        public Guid TriageRecordId { get; set; }
        public TriageRecord TriageRecord { get; set; } = null!;

        // ── Doctor Info ──────────────────────────────────────────────────────
        /// <summary>Guid of the doctor who approved and issued the prescription.</summary>
        public Guid? IssuedByDoctorId { get; set; }

        // ── Prescription Lifecycle ───────────────────────────────────────────
        /// <summary>
        /// Workflow state: "Draft" → "Issued" → "Dispensed" | "Cancelled"
        /// </summary>
        public string Status { get; set; } = "Draft";

        /// <summary>Doctor's additional notes on the prescription.</summary>
        public string? Notes { get; set; }

        // ── AI Validation/Safety Agent Results ───────────────────────────────
        /// <summary>
        /// JSON summary produced by the Validation/Safety Agent.
        /// e.g. { "warnings": ["Drug A + Drug B interaction"], "emergencyFlag": false }
        /// </summary>
        public string? AiSafetyCheckResult { get; set; }

        /// <summary>
        /// Aggregated safety verdict: "Safe" | "Warning" | "Blocked"
        /// "Blocked" means the AI has flagged a critical issue – doctor must review before issuing.
        /// </summary>
        public string? AiSafetyStatus { get; set; }

        // ── Notification Tracking ────────────────────────────────────────────
        /// <summary>Whether the patient has been notified of the issued prescription.</summary>
        public bool NotificationSent { get; set; } = false;

        /// <summary>Simulated notification log entry (channel: "Email" | "SMS" | null).</summary>
        public string? NotificationChannel { get; set; }

        public DateTime? NotifiedAt { get; set; }

        // ── Audit Fields ─────────────────────────────────────────────────────
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // ── Navigation ────────────────────────────────────────────────────────
        public ICollection<PrescriptionItem> Items { get; set; } = new List<PrescriptionItem>();
    }
}
