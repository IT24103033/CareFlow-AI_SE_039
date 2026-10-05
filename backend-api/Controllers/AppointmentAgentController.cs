using System.Security.Claims;
using CareFlowAI.API.Data;
using CareFlowAI.Orchestrator;
using CareFlowAI.Orchestrator.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareFlowAI.API.Controllers
{
    [ApiController]
    [Route("api/AppointmentAgent")]
    [Authorize(Roles = "Patient")]
    public class AppointmentAgentController : ControllerBase
    {
        private readonly AppointmentWorkflowRunner _workflowRunner;
        private readonly ApplicationDbContext _context;

        public AppointmentAgentController(
            AppointmentWorkflowRunner workflowRunner,
            ApplicationDbContext context)
        {
            _workflowRunner = workflowRunner;
            _context = context;
        }

        [HttpPost("book")]
        public async Task<IActionResult> Book(
            [FromBody] AppointmentAgentRequest request)
        {
            var userIdValue =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;

            if (!Guid.TryParse(userIdValue, out var userId))
            {
                return Unauthorized(new
                {
                    success = false,
                    status = "AuthenticationFailed",
                    message = "Invalid authenticated user."
                });
            }

            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return Unauthorized(new
                {
                    success = false,
                    status = "AuthenticationFailed",
                    message = "Authenticated user could not be found."
                });
            }

            if (!user.PatientProfileId.HasValue)
            {
                return Unauthorized(new
                {
                    success = false,
                    status = "AuthenticationFailed",
                    message = "Authenticated patient profile could not be found."
                });
            }

            // Patient ID is derived from the authenticated user.
            // The PatientId supplied by the client is never trusted.
            var patientId = user.PatientProfileId.Value;

            try
            {
                // Create the typed B → C workflow request.
                var workflowRequest = new AppointmentWorkflowRequest(
                    request.WorkflowId,
                    patientId,
                    request.DoctorId,
                    request.AppointmentDate,
                    request.StartTime,
                    request.EndTime);

                var result =
                    await _workflowRunner.RunAsync(workflowRequest);

                switch (result.Status)
                {
                    case AppointmentActionStatus.Success:
                        return Ok(new
                        {
                            success = true,
                            status = result.Status.ToString(),
                            message = result.Message,
                            workflowId = result.WorkflowId,
                            appointmentId = result.AppointmentId
                        });

                    case AppointmentActionStatus.Unavailable:
                        return Conflict(new
                        {
                            success = false,
                            status = result.Status.ToString(),
                            message = result.Message,
                            workflowId = result.WorkflowId,
                            appointmentId = result.AppointmentId
                        });

                    case AppointmentActionStatus.Conflict:
                        return Conflict(new
                        {
                            success = false,
                            status = result.Status.ToString(),
                            message = result.Message,
                            workflowId = result.WorkflowId,
                            appointmentId = result.AppointmentId
                        });

                    case AppointmentActionStatus.InvalidRequest:
                        return BadRequest(new
                        {
                            success = false,
                            status = result.Status.ToString(),
                            message = result.Message,
                            workflowId = result.WorkflowId,
                            appointmentId = result.AppointmentId
                        });

                    case AppointmentActionStatus.ProviderError:
                        return StatusCode(
                            StatusCodes.Status502BadGateway,
                            new
                            {
                                success = false,
                                status = result.Status.ToString(),
                                message = result.Message,
                                workflowId = result.WorkflowId,
                                appointmentId = result.AppointmentId
                            });

                    default:
                        return StatusCode(
                            StatusCodes.Status500InternalServerError,
                            new
                            {
                                success = false,
                                status = "UnknownError",
                                message =
                                    "An unknown appointment workflow result was returned.",
                                workflowId = result.WorkflowId,
                                appointmentId = result.AppointmentId
                            });
                }
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    status = "InvalidRequest",
                    message = ex.Message
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    success = false,
                    status = "NotFound",
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    success = false,
                    status = "Conflict",
                    message = ex.Message
                });
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        success = false,
                        status = "ProviderError",
                        message =
                            "The appointment action agent could not complete the request."
                    });
            }
        }
    }

    public class AppointmentAgentRequest
    {
        public Guid WorkflowId { get; set; }

        public Guid DoctorId { get; set; }

        // Retained for compatibility, but NOT trusted.
        // The patient ID is always derived from the authenticated JWT user.
        public Guid PatientId { get; set; }

        public DateOnly AppointmentDate { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }
    }
}
