using System;
using System.Threading;
using System.Threading.Tasks;
using CareFlowAI.API.Services;

namespace CareFlowAI.API.Services
{
    public class DomainContextWrapper : CareFlowAI.Orchestrator.Agents.IDomainContextAdapter
    {
        private readonly IPatientContextTool _tool;
        public DomainContextWrapper(IPatientContextTool tool) => _tool = tool;
        public async Task<CareFlowAI.Orchestrator.Agents.AgentPatientProfile?> GetPatientProfileAsync(Guid patientId, CancellationToken cancellationToken)
        {
            var p = await _tool.GetPatientProfileAsync(patientId, cancellationToken);
            return p == null ? null : new CareFlowAI.Orchestrator.Agents.AgentPatientProfile { MedicalHistorySummary = p.MedicalHistorySummary };
        }
    }
}
