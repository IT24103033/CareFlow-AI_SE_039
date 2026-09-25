using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CareFlowAI.API.DTOs
{
    // ── Prescription DTOs ─────────────────────────────────────────────────────

    /// <summary>DTO for a single medicine line-item in a prescription.</summary>
    public class PrescriptionItemDto
    {
        [Required]
        public Guid MedicineId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [MaxLength(200)]
        public string Dosage { get; set; } = string.Empty;

        [Range(1, 365)]
        public int DurationDays { get; set; } = 1;
    }

    /// <summary>DTO for creating a new prescription (initially Draft status).</summary>
    public class CreatePrescriptionDto
    {
        [Required]
        public Guid PatientId { get; set; }

        [Required]
        public Guid TriageRecordId { get; set; }

        public string? Notes { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one medicine item is required.")]
        public List<PrescriptionItemDto> Items { get; set; } = new();
    }

    /// <summary>DTO for updating prescription notes or doctor info.</summary>
    public class UpdatePrescriptionDto
    {
        public string? Notes { get; set; }
        public Guid? IssuedByDoctorId { get; set; }
    }

    /// <summary>DTO for doctor approval/rejection of an AI-validated prescription.</summary>
    public class ApprovePrescriptionDto
    {
        /// <summary>Doctor GUID who is approving the prescription.</summary>
        [Required]
        public Guid DoctorId { get; set; }

        /// <summary>"Approved" or "Rejected"</summary>
        [Required]
        public string Decision { get; set; } = string.Empty;

        public string? DoctorNotes { get; set; }
    }
}
