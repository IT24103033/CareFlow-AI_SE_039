using System;

namespace CareFlowAI.API.Models
{
    public class Admission
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid PatientProfileId { get; set; }
        public PatientProfile PatientProfile { get; set; } = null!;
        public Guid WardId { get; set; }
        public Ward Ward { get; set; } = null!;
        public DateTime AdmittedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DischargedAt { get; set; }
        public string RiskLevel { get; set; } = "Pending AI Analysis"; 
        public string Status { get; set; } = "Admitted";
    }
}