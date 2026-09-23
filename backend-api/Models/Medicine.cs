using System;

namespace CareFlowAI.API.Models
{
    /// <summary>
    /// Represents a medicine/drug entry in the hospital pharmacy catalog.
    /// Component D: Pharmacy Inventory &amp; E-Prescriptions (Amodhya).
    /// </summary>
    public class Medicine
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // ── Drug Identity ────────────────────────────────────────────────────
        /// <summary>Full name including strength, e.g. "Amoxicillin 500mg".</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Category: "Antibiotic" | "Painkiller" | "Antiviral" | etc.</summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>Description of the drug, indications, and general notes.</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>Manufacturer or supplier name.</summary>
        public string Manufacturer { get; set; } = string.Empty;

        // ── Stock Management ─────────────────────────────────────────────────
        /// <summary>Current number of units in stock.</summary>
        public int StockQuantity { get; set; } = 0;

        /// <summary>Stock level below which a low-stock alert is raised.</summary>
        public int ReorderLevel { get; set; } = 10;

        /// <summary>Price per unit in LKR.</summary>
        public decimal UnitPrice { get; set; } = 0;

        /// <summary>Expiry date of the current batch.</summary>
        public DateOnly ExpiryDate { get; set; }

        // ── Lifecycle ────────────────────────────────────────────────────────
        /// <summary>Soft-delete flag. False means deactivated from catalog.</summary>
        public bool IsActive { get; set; } = true;

        // ── Audit Fields ─────────────────────────────────────────────────────
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // ── Navigation ────────────────────────────────────────────────────────
        public ICollection<PrescriptionItem> PrescriptionItems { get; set; } = new List<PrescriptionItem>();
    }
}
