
using CareFlowAI.API.DTOs;
using CareFlowAI.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareFlowAI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorAvailabilityController : ControllerBase
    {
        private readonly DoctorAvailabilityService _service;

        public DoctorAvailabilityController(
            DoctorAvailabilityService service)
        {
            _service = service;
        }

        // GET: api/DoctorAvailability
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<DoctorAvailabilityDto>>> GetAll()
        {
            var availability = await _service.GetAllAsync();

            return Ok(availability);
        }

        // GET: api/DoctorAvailability/{doctorId}
        [Authorize]
        [HttpGet("{doctorId:guid}")]
        public async Task<ActionResult<List<DoctorAvailabilityDto>>> GetByDoctorId(
            Guid doctorId)
        {
            var availability = await _service.GetByDoctorIdAsync(doctorId);

            return Ok(availability);
        }

        // GET: api/DoctorAvailability/search
        [HttpGet("search")]
        public async Task<ActionResult<List<DoctorAvailabilityDto>>> Search(
            [FromQuery] string? specialization,
            [FromQuery] Guid? doctorId,
            [FromQuery] DateOnly? date)
        {
            var availability = await _service.SearchAsync(
                specialization,
                doctorId,
                date);

            return Ok(availability);
        }

        // GET: api/DoctorAvailability/slots
        [HttpGet("slots")]
        public async Task<ActionResult<List<AvailableSlotDto>>> GetAvailableSlots(
            [FromQuery] Guid doctorId,
            [FromQuery] DateOnly date,
            [FromQuery] int slotDurationMinutes = 30)
        {
            try
            {
                var slots = await _service.GetAvailableSlotsAsync(
                    doctorId,
                    date,
                    slotDurationMinutes);

                if (slots.Count == 0)
                {
                    return NotFound(
                        "No availability found for the selected doctor and date.");
                }

                return Ok(slots);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: api/DoctorAvailability
        [Authorize(Roles = "Doctor,Admin")]
        [HttpPost]
        public async Task<ActionResult<DoctorAvailabilityDto>> Create(
            CreateDoctorAvailabilityDto dto)
        {
            if (User?.IsInRole("Doctor") == true)
            {
                var docClaim = User?.FindFirst("doctor_id")?.Value;
                if (!Guid.TryParse(docClaim, out var authDocId) || dto.DoctorId != authDocId)
                    return StatusCode(403, "You can only manage your own availability schedule.");
            }

            try
            {
                var result = await _service.CreateAsync(dto);

                if (result == null)
                {
                    return NotFound("Doctor not found or inactive.");
                }

                return CreatedAtAction(
                    nameof(GetByDoctorId),
                    new { doctorId = result.DoctorId },
                    result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        // PUT: api/DoctorAvailability/{id}
        [Authorize(Roles = "Doctor,Admin")]
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<DoctorAvailabilityDto>> Update(
            Guid id,
            UpdateDoctorAvailabilityDto dto)
        {
            if (User?.IsInRole("Doctor") == true)
            {
                var existing = await _service.GetByIdAsync(id);
                if (existing == null) return NotFound("Availability record not found.");

                var docClaim = User?.FindFirst("doctor_id")?.Value;
                if (!Guid.TryParse(docClaim, out var authDocId) || existing.DoctorId != authDocId)
                    return StatusCode(403, "You can only manage your own availability schedule.");
            }

            try
            {
                var result = await _service.UpdateAsync(id, dto);

                if (result == null)
                {
                    return NotFound("Availability record not found.");
                }

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        // DELETE: api/DoctorAvailability/{id}
        [Authorize(Roles = "Doctor,Admin")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (User?.IsInRole("Doctor") == true)
            {
                var existing = await _service.GetByIdAsync(id);
                if (existing == null) return NotFound("Availability record not found.");

                var docClaim = User?.FindFirst("doctor_id")?.Value;
                if (!Guid.TryParse(docClaim, out var authDocId) || existing.DoctorId != authDocId)
                    return StatusCode(403, "You can only manage your own availability schedule.");
            }

            var deleted = await _service.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound("Availability record not found.");
            }

            return NoContent();
        }
    }
}
