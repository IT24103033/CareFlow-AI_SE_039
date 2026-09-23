using System;
using System.ComponentModel.DataAnnotations;

namespace CareFlowAI.API.Models
{
    /// <summary>
    /// DTO used when creating a new admission — only IDs needed, no nav-property validation.
    /// </summary>
    public class AdmissionRequestDto
    {
        [Required]
        public Guid PatientProfileId { get; set; }

        [Required]
        public Guid WardId { get; set; }
    }
}
