using CareFlowAI.Orchestrator;
using Microsoft.AspNetCore.Mvc;

namespace CareFlowAI.API.Controllers
{
    [ApiController]
    [Route("api/AppointmentAgent")]
    public class AppointmentAgentController : ControllerBase
    {
        private readonly AppointmentWorkflowRunner _workflowRunner;

        public AppointmentAgentController(
            AppointmentWorkflowRunner workflowRunner)
        {
            _workflowRunner = workflowRunner;
        }

        // Component C: Appointment Action Agent
        // Runs the allow-listed appointment workflow:
        // 1. Find available slots
        // 2. Check booking conflict
        // 3. Create tentative booking
        [HttpPost("book")]
        public async Task<IActionResult> Book(
            [FromBody] AppointmentAgentRequest request)
        {
            try
            {
                var result = await _workflowRunner.RunAsync(
                    request.DoctorId,
                    request.PatientId,
                    request.AppointmentDate,
                    request.StartTime,
                    request.EndTime);

                return Ok(new
                {
                    success = true,
                    result = result
                });
            }
            catch (Exception)
            {
                return StatusCode(
                    500,
                    "The appointment action agent could not complete the request.");
            }
        }
    }

    public class AppointmentAgentRequest
    {
        public Guid DoctorId { get; set; }

        public Guid PatientId { get; set; }

        public DateOnly AppointmentDate { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }
    }
}
