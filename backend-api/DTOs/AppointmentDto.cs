namespace CareFlowAI.API.DTOs
{
    public class AppointmentDto
    {
        public Guid Id { get; set; }

        public Guid DoctorId { get; set; }

        public string DoctorName { get; set; } = string.Empty;

        public Guid PatientId { get; set; }

        public DateOnly AppointmentDate { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}