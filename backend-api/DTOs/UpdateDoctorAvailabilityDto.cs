using System;
using System.ComponentModel.DataAnnotations;

namespace CareFlowAI.API.DTOs
{
    public class UpdateDoctorAvailabilityDto
    {
        [Required]
        public DateOnly Date { get; set; }

        [Required]
        public TimeOnly StartTime { get; set; }

        [Required]
        public TimeOnly EndTime { get; set; }
    }
}