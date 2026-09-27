using System;
using System.ComponentModel.DataAnnotations;

namespace CareFlowAI.API.DTOs
{
    public class LoginRequestDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string TokenType { get; set; } = "Bearer";
        public DateTime ExpiresAt { get; set; }
        public AuthUserDto User { get; set; } = new();
    }

    public class AuthUserDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public Guid? DoctorId { get; set; }
        public Guid? PatientId { get; set; }
        public string FullName { get; set; } = string.Empty;
    }

    public class RegisterPatientRequestDto
    {
        [Required]
        [MinLength(3, ErrorMessage = "Username must be at least 3 characters.")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public DateOnly DateOfBirth { get; set; }

        public string BloodGroup { get; set; } = "O+";

        public string MedicalHistorySummary { get; set; } = "Registered via Mobile App";
    }

    public class RegisterStaffRequestDto
    {
        [Required]
        [MinLength(3)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty; // "Staff" or "Doctor"

        public string FullName { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
