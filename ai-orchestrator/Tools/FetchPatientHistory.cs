using System.Threading.Tasks;

namespace CareFlowAI.Orchestrator.Tools
{
    /// <summary>
    /// Kept for compatibility. The API supplies a database-backed implementation
    /// of <see cref="IPatientHistoryTool"/> so the agent does not HTTP-call itself.
    /// </summary>
    public class FetchPatientHistory : IPatientHistoryTool
    {
        public Task<string> ExecuteAsync(string patientName)
        {
            return Task.FromResult(
                $"No patient-history provider is configured for '{patientName}'.");
        }
    }
}
