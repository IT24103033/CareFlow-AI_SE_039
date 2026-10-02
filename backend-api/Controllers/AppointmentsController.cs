using System.Security.Claims;
using CareFlowAI.API.Data;
using CareFlowAI.API.DTOs;
using CareFlowAI.API.Models;
using CareFlowAI.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareFlowAI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AppointmentsController : ControllerBase
    {
        private readonly AppointmentService _service;
        private readonly ApplicationDbContext _context;

        public AppointmentsController(
            AppointmentService service,
            ApplicationDbContext context)
        {
            _service = service;
            _context = context;
        }

        // ============================================================
        // Helper methods
        // ============================================================

        // Gets the authenticated application's User ID from the JWT.
        private Guid? GetCurrentUserId()
        {
            var userIdValue =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;

            if (Guid.TryParse(userIdValue, out var userId))
            {
                return userId;
            }

            return null;
        }

        // Gets the PatientProfile ID linked to the authenticated user.
        private async Task<Guid?> GetCurrentPatientIdAsync()
        {
            var userId = GetCurrentUserId();

            if (!userId.HasValue)
            {
                return null;
            }

            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId.Value);

            return user?.PatientProfileId;
        }

        // Gets the Doctor ID linked to the authenticated user.
        private async Task<Guid?> GetCurrentDoctorIdAsync()
        {
            var userId = GetCurrentUserId();

            if (!userId.HasValue)
            {
                return null;
            }

            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId.Value);

            return user?.DoctorId;
        }

        // ============================================================
        // GET: api/Appointments
        // ============================================================

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<AppointmentDto>>> GetAll()
        {
            var appointments = await _service.GetAllAsync();

            return Ok(appointments);
        }

        // ============================================================
        // GET: api/Appointments/{id}
        // ============================================================

        [HttpGet("{id}")]
        public async Task<ActionResult<AppointmentDto>> GetById(Guid id)
        {
            var appointment = await _service.GetByIdAsync(id);

            if (appointment == null)
            {
                return NotFound("Appointment not found.");
            }

            // Admin can access any appointment.
            if (User.IsInRole("Admin"))
            {
                return Ok(appointment);
            }

            // Patient can only access their own appointment.
            if (User.IsInRole("Patient"))
            {
                var patientId = await GetCurrentPatientIdAsync();

                if (!patientId.HasValue ||
                    appointment.PatientId != patientId.Value)
                {
                    return Forbid();
                }

                return Ok(appointment);
            }

            // Doctor can only access appointments assigned to them.
            if (User.IsInRole("Doctor"))
            {
                var doctorId = await GetCurrentDoctorIdAsync();

                if (!doctorId.HasValue ||
                    appointment.DoctorId != doctorId.Value)
                {
                    return Forbid();
                }

                return Ok(appointment);
            }

            return Forbid();
        }

        // ============================================================
        // GET: api/Appointments/patient/{patientId}
        // ============================================================

        [HttpGet("patient/{patientId}")]
        public async Task<ActionResult<List<AppointmentDto>>> GetByPatient(
            Guid patientId)
        {
            // Admin can view any patient's appointments.
            if (User.IsInRole("Admin"))
            {
                var adminAppointments =
                    await _service.GetByPatientIdAsync(patientId);

                return Ok(adminAppointments);
            }

            // Patients can only request their own appointments.
            if (User.IsInRole("Patient"))
            {
                var currentPatientId =
                    await GetCurrentPatientIdAsync();

                if (!currentPatientId.HasValue)
                {
                    return Unauthorized(
                        "Authenticated patient profile could not be found.");
                }

                if (currentPatientId.Value != patientId)
                {
                    return Forbid();
                }

                var appointments =
                    await _service.GetByPatientIdAsync(
                        currentPatientId.Value);

                return Ok(appointments);
            }

            // Doctors should not use the patient endpoint
            // to access arbitrary patient appointment data.
            return Forbid();
        }

        // ============================================================
        // POST: api/Appointments/check-conflict
        // ============================================================

        [HttpPost("check-conflict")]
        [Authorize(Roles = "Patient,Doctor,Admin")]
        public async Task<ActionResult<bool>> CheckConflict(
            [FromBody] CreateAppointmentDto dto)
        {
            if (dto.StartTime >= dto.EndTime)
            {
                return BadRequest(
                    "Start time must be earlier than end time.");
            }

            // Patients may only check conflicts for themselves.
            if (User.IsInRole("Patient"))
            {
                var patientId = await GetCurrentPatientIdAsync();

                if (!patientId.HasValue)
                {
                    return Unauthorized(
                        "Authenticated patient profile could not be found.");
                }

                if (dto.PatientId != patientId.Value)
                {
                    return Forbid();
                }
            }

            // Doctors may only check conflicts for their own doctor ID.
            if (User.IsInRole("Doctor"))
            {
                var doctorId = await GetCurrentDoctorIdAsync();

                if (!doctorId.HasValue)
                {
                    return Unauthorized(
                        "Authenticated doctor profile could not be found.");
                }

                if (dto.DoctorId != doctorId.Value)
                {
                    return Forbid();
                }
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

        // ============================================================
        // POST: api/Appointments/tentative
        // ============================================================

        [HttpPost("tentative")]
        [Authorize(Roles = "Patient")]
        public async Task<ActionResult<AppointmentDto>> CreateTentative(
            [FromBody] CreateAppointmentDto dto)
        {
            var currentPatientId =
                await GetCurrentPatientIdAsync();

            if (!currentPatientId.HasValue)
            {
                return Unauthorized(
                    "Authenticated patient profile could not be found.");
            }

            // Never trust the patient ID supplied by the client.
            // Always use the authenticated patient's ID.
            dto.PatientId = currentPatientId.Value;

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

        // ============================================================
        // POST: api/Appointments/{id}/approve
        // ============================================================

        [HttpPost("{id}/approve")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult<AppointmentDto>> Approve(Guid id)
        {
            var currentDoctorId =
                await GetCurrentDoctorIdAsync();

            if (!currentDoctorId.HasValue)
            {
                return Unauthorized(
                    "Authenticated doctor profile could not be found.");
            }

            // Get the appointment first so we can verify
            // that the authenticated doctor owns it.
            var appointment =
                await _service.GetByIdAsync(id);

            if (appointment == null)
            {
                return NotFound("Appointment not found.");
            }

            // A doctor can only approve appointments
            // assigned to that doctor.
            if (appointment.DoctorId != currentDoctorId.Value)
            {
                return Forbid();
            }

            try
            {
                var approved =
                    await _service.ApproveAsync(
                        id,
                        currentDoctorId.Value);

                if (approved == null)
                {
                    return NotFound("Appointment not found.");
                }

                return Ok(approved);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // ============================================================
        // POST: api/Appointments/{id}/confirm
        // ============================================================

        [HttpPost("{id}/confirm")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<ActionResult<AppointmentDto>> Confirm(Guid id)
        {
            var appointment = await _service.GetByIdAsync(id);

            if (appointment == null)
            {
                return NotFound("Appointment not found.");
            }

            // Doctors can only confirm their own appointments.
            if (User.IsInRole("Doctor"))
            {
                var doctorId = await GetCurrentDoctorIdAsync();

                if (!doctorId.HasValue)
                {
                    return Unauthorized(
                        "Authenticated doctor profile could not be found.");
                }

                if (appointment.DoctorId != doctorId.Value)
                {
                    return Forbid();
                }
            }

            try
            {
                var confirmed =
                    await _service.ConfirmAsync(id);

                if (confirmed == null)
                {
                    return NotFound("Appointment not found.");
                }

                return Ok(confirmed);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // ============================================================
        // POST: api/Appointments/{id}/cancel
        // ============================================================

        [HttpPost("{id}/cancel")]
        [Authorize(Roles = "Patient,Doctor,Admin")]
        public async Task<ActionResult<AppointmentDto>> Cancel(Guid id)
        {
            var appointment = await _service.GetByIdAsync(id);

            if (appointment == null)
            {
                return NotFound("Appointment not found.");
            }

            // Patient can only cancel their own appointment.
            if (User.IsInRole("Patient"))
            {
                var patientId =
                    await GetCurrentPatientIdAsync();

                if (!patientId.HasValue)
                {
                    return Unauthorized(
                        "Authenticated patient profile could not be found.");
                }

                if (appointment.PatientId != patientId.Value)
                {
                    return Forbid();
                }
            }

            // Doctor can only cancel appointments assigned to them.
            if (User.IsInRole("Doctor"))
            {
                var doctorId =
                    await GetCurrentDoctorIdAsync();

                if (!doctorId.HasValue)
                {
                    return Unauthorized(
                        "Authenticated doctor profile could not be found.");
                }

                if (appointment.DoctorId != doctorId.Value)
                {
                    return Forbid();
                }
            }

            var cancelled =
                await _service.CancelAsync(id);

            if (cancelled == null)
            {
                return NotFound("Appointment not found.");
            }

            return Ok(cancelled);
        }

        // ============================================================
        // DELETE: api/Appointments/{id}
        // ============================================================

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
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