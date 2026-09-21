using System;
using System.ComponentModel.DataAnnotations;

namespace CareFlowAI.API.DTOs
{
    // ── Medicine DTOs ─────────────────────────────────────────────────────────

    /// <summary>DTO for creating a new medicine in the catalog.</summary>
    public class CreateMedicineDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Category { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [MaxLength(150)]
        public string Manufacturer { get; set; } = string.Empty;

        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; } = 0;

        [Range(0, int.MaxValue)]
        public int ReorderLevel { get; set; } = 10;

        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; } = 0;

        public DateOnly ExpiryDate { get; set; }
    }

    /// <summary>DTO for updating an existing medicine entry.</summary>
    public class UpdateMedicineDto
    {
        [MaxLength(200)]
        public string? Name { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; }

        public string? Description { get; set; }

        [MaxLength(150)]
        public string? Manufacturer { get; set; }

        [Range(0, int.MaxValue)]
        public int? ReorderLevel { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? UnitPrice { get; set; }

        public DateOnly? ExpiryDate { get; set; }

        public bool? IsActive { get; set; }
    }

    /// <summary>DTO for restocking a medicine (adding units to stock).</summary>
    public class RestockMedicineDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity to add must be at least 1.")]
        public int QuantityToAdd { get; set; }
    }
}
