using System;
using System.Collections.Generic;

namespace CareFlowAI.API.Models
{
    public class Doctor
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string FullName { get; set; } = string.Empty;

        public string Specialization { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public ICollection<DoctorAvailability> Availabilities { get; set; }
            = new List<DoctorAvailability>();

        public ICollection<Appointment> Appointments { get; set; }
            = new List<Appointment>();
    }
}