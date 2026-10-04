using System.Threading;
using System.Threading.Tasks;

namespace CareFlowAI.Orchestrator.Handoff
{
    public class ComponentBHandoffPayload
    {
        public string PatientName { get; set; } = string.Empty;
        public string CurrentSymptoms { get; set; } = string.Empty;
        public string PatientHistory { get; set; } = string.Empty;
        public string RiskLevel { get; set; } = string.Empty;
        public string[] FlaggedFactors { get; set; } = System.Array.Empty<string>();
        public string RecommendedWardType { get; set; } = string.Empty;
        public bool OutputValid { get; set; }
    }

    public interface IComponentBHandoff
    {
        Task ReceiveAsync(ComponentBHandoffPayload payload, CancellationToken cancellationToken = default);
    }
}
