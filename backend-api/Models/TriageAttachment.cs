using System;
using System.ComponentModel.DataAnnotations;

namespace CareFlowAI.API.Models
{
    public class TriageAttachment
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string CloudinaryPublicId { get; set; } = string.Empty;

        [Required]
        public Guid UploaderId { get; set; } // PatientId or DoctorId

        public Guid? TriageRecordId { get; set; }
        public TriageRecord? TriageRecord { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
