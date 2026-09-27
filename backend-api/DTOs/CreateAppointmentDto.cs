namespace CareFlowAI.API.DTOs
{
    public class CreateAppointmentDto
    {
        public Guid DoctorId { get; set; }

        public Guid PatientId { get; set; }

        public DateOnly AppointmentDate { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }
    }
}