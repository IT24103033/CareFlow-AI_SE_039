using System;

namespace CareFlowAI.API.Models
{
    public class DoctorAvailability
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid DoctorId { get; set; }

        public DateOnly Date { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }

        // Navigation property
        public Doctor Doctor { get; set; } = null!;
    }
}