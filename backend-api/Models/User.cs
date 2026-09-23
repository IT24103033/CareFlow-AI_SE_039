using System;
using System.ComponentModel.DataAnnotations;

namespace CareFlowAI.API.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty; // Store plain for testing, should be hashed
        public string Role { get; set; } = string.Empty; // "Admin", "Doctor", "Staff"
    }
}
