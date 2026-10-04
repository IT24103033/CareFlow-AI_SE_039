using System.ComponentModel.DataAnnotations;

namespace CareFlowAI.API.Models
{
    public class UserUpsertDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        public string? Password { get; set; }

        [Required]
        public string Role { get; set; } = "Staff";
    }
}
