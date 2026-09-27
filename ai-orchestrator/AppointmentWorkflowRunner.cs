using CareFlowAI.Orchestrator.Agents;
using CareFlowAI.Orchestrator.Tools;

namespace CareFlowAI.Orchestrator
{
    public class AppointmentWorkflowRunner
    {
        private readonly AppointmentActionAgent _agent;

        public AppointmentWorkflowRunner()
        {
            // Create the HTTP client used by the appointment tools
            var httpClient = new HttpClient();

            // Create the allow-listed tools
            var findAvailableSlotsTool =
                new FindAvailableSlotsTool(httpClient);

            var checkBookingConflictTool =
                new CheckBookingConflictTool(httpClient);

            var createTentativeBookingTool =
                new CreateTentativeBookingTool(httpClient);

            // Create the Component C Action Agent
            _agent = new AppointmentActionAgent(
                findAvailableSlotsTool,
                checkBookingConflictTool,
                createTentativeBookingTool);
        }

        // Runs the controlled appointment workflow.
        public async Task<string> RunAsync(
            Guid doctorId,
            Guid patientId,
            DateOnly appointmentDate,
            TimeOnly startTime,
            TimeOnly endTime)
        {
            return await _agent.FindAndBookAsync(
                doctorId,
                patientId,
                appointmentDate,
                startTime,
                endTime);
        }
    }
}