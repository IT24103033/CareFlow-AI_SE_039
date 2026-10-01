using CareFlowAI.API.DTOs;
using CareFlowAI.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace CareFlowAI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly AppointmentService _service;

        public AppointmentsController(AppointmentService service)
        {
            _service = service;
        }

        // GET: api/Appointments
        [HttpGet]
        public async Task<ActionResult<List<AppointmentDto>>> GetAll()
        {
            var appointments = await _service.GetAllAsync();

            return Ok(appointments);
        }

        // GET: api/Appointments/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<AppointmentDto>> GetById(Guid id)
        {
            var appointment = await _service.GetByIdAsync(id);

            if (appointment == null)
            {
                return NotFound("Appointment not found.");
            }

            return Ok(appointment);
        }

        // GET: api/Appointments/patient/{patientId}
        [HttpGet("patient/{patientId}")]
        public async Task<ActionResult<List<AppointmentDto>>> GetByPatient(
            Guid patientId)
        {
            var appointments =
                await _service.GetByPatientIdAsync(patientId);

            return Ok(appointments);
        }

        // POST: api/Appointments/check-conflict
        [HttpPost("check-conflict")]
        public async Task<ActionResult<bool>> CheckConflict(
            [FromBody] CreateAppointmentDto dto)
        {
            if (dto.StartTime >= dto.EndTime)
            {
                return BadRequest(
                    "Start time must be earlier than end time.");
            }

            var conflict = await _service.CheckConflictAsync(
                dto.DoctorId,
                dto.AppointmentDate,
                dto.StartTime,
                dto.EndTime);

            return Ok(new
            {
                hasConflict = conflict
            });
        }

        // POST: api/Appointments/tentative
        [HttpPost("tentative")]
        public async Task<ActionResult<AppointmentDto>> CreateTentative(
            [FromBody] CreateAppointmentDto dto)
        {
            try
            {
                var appointment =
                    await _service.CreateTentativeAsync(dto);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = appointment.Id },
                    appointment);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        // POST: api/Appointments/{id}/confirm
        [HttpPost("{id}/confirm")]
        public async Task<ActionResult<AppointmentDto>> Confirm(Guid id)
        {
            try
            {
                var appointment =
                    await _service.ConfirmAsync(id);

                if (appointment == null)
                {
                    return NotFound("Appointment not found.");
                }

                return Ok(appointment);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        // POST: api/Appointments/{id}/cancel
        [HttpPost("{id}/cancel")]
        public async Task<ActionResult<AppointmentDto>> Cancel(Guid id)
        {
            var appointment =
                await _service.CancelAsync(id);

            if (appointment == null)
            {
                return NotFound("Appointment not found.");
            }

            return Ok(appointment);
        }

        // DELETE: api/Appointments/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _service.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound("Appointment not found.");
            }

            return NoContent();
        }
    }
}