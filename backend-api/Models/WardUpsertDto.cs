using System.ComponentModel.DataAnnotations;

namespace CareFlowAI.API.Models
{
    public class WardUpsertDto
    {
        [Required]
        public string WardNumber { get; set; } = string.Empty;

        [Required]
        public string WardType { get; set; } = string.Empty;

        [Range(1, 500)]
        public int Capacity { get; set; }
    }
}
