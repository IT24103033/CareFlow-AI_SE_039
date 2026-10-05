using System;

namespace CareFlowAI.API.Models
{
    public class Appointment
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid DoctorId { get; set; }

        public Guid PatientId { get; set; }

        public DateOnly AppointmentDate { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }

        public string Status { get; set; } = "Tentative";

        // Human-in-the-loop approval state.
        // A tentative appointment must be approved by a doctor
        // before it can become Confirmed.
        public string ApprovalStatus { get; set; } = "Pending";

        // ID of the doctor who approved the appointment.
        public Guid? ApprovedByDoctorId { get; set; }

        // Timestamp of the doctor's approval.
        public DateTime? ApprovedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Doctor Doctor { get; set; } = null!;

        public PatientProfile Patient { get; set; } = null!;
    }
}