using System;
using System.ComponentModel.DataAnnotations;

namespace CareFlowAI.API.Models
{
    public class PatientProfileUpdateDto
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public DateOnly DateOfBirth { get; set; }

        [Required]
        public string BloodGroup { get; set; } = string.Empty;

        public string MedicalHistorySummary { get; set; } = string.Empty;
    }
}
