using System.Threading.Tasks;

namespace CareFlowAI.Orchestrator.Tools
{
    public interface IPatientHistoryTool
    {
        Task<string> ExecuteAsync(string patientName);
    }
}
