using System;

namespace CareFlowAI.API.Models
{
    /// <summary>
    /// A single medicine line-item on an e-prescription.
    /// Component D: Pharmacy Inventory &amp; E-Prescriptions (Amodhya).
    /// </summary>
    public class PrescriptionItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // ── Foreign Keys ─────────────────────────────────────────────────────
        public Guid PrescriptionId { get; set; }
        public Prescription Prescription { get; set; } = null!;

        public Guid MedicineId { get; set; }
        public Medicine Medicine { get; set; } = null!;

        // ── Dosage Details ────────────────────────────────────────────────────
        /// <summary>Number of units to dispense.</summary>
        public int Quantity { get; set; }

        /// <summary>Instructions e.g. "1 tablet twice daily after meals".</summary>
        public string Dosage { get; set; } = string.Empty;

        /// <summary>How many days the course runs for.</summary>
        public int DurationDays { get; set; }
    }
}
