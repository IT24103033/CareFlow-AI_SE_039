using CareFlowAI.Orchestrator.Agents;
using CareFlowAI.Orchestrator.Models;

namespace CareFlowAI.Orchestrator
{
    public class AppointmentWorkflowRunner
    {
        private readonly AppointmentActionAgent _agent;

        public AppointmentWorkflowRunner(AppointmentActionAgent agent)
        {
            _agent = agent;
        }

        public async Task<AppointmentWorkflowResult> RunAsync(
            AppointmentWorkflowRequest request)
        {
            var result = await _agent.FindAndBookAsync(
                request.DoctorId,
                request.PatientId,
                request.AppointmentDate,
                request.StartTime,
                request.EndTime);

            return new AppointmentWorkflowResult(
                request.WorkflowId,
                result.AppointmentId,
                result.Status,
                result.Message);
        }
    }
}