using System;
using System.ComponentModel.DataAnnotations;

namespace CareFlowAI.API.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty; // Securely hashed password
        public string Role { get; set; } = string.Empty; // "Admin", "Doctor", "Staff", "Patient"
        public Guid? DoctorId { get; set; }
        public Guid? PatientProfileId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
