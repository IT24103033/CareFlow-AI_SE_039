using System;
using System.Collections.Generic;

namespace CareFlowAI.API.Models
{
    public class PatientProfile
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FullName { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        public string BloodGroup { get; set; } = string.Empty;
        public string MedicalHistorySummary { get; set; } = string.Empty;

        // ── Contact Details (used by Component D notification service) ───────
        /// <summary>Patient email address for prescription notifications. Optional.</summary>
        public string? Email { get; set; }

        /// <summary>Patient mobile phone number (E.164 format, e.g. +94771234567) for SMS notifications. Optional.</summary>
        public string? Phone { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Property for their admissions
        public ICollection<Admission> Admissions { get; set; } = new List<Admission>();
    }
}