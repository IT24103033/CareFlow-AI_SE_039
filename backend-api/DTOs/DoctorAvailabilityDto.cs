using System;

namespace CareFlowAI.API.DTOs
{
    public class DoctorAvailabilityDto
    {
        public Guid Id { get; set; }

        public Guid DoctorId { get; set; }

        public string DoctorName { get; set; } = string.Empty;

        public string Specialization { get; set; } = string.Empty;

        public DateOnly Date { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }
    }
}