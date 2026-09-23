using CareFlowAI.API.DTOs;
using CareFlowAI.API.Services;
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
        [HttpGet]
        public async Task<ActionResult<List<DoctorAvailabilityDto>>> GetAll()
        {
            var availability = await _service.GetAllAsync();

            return Ok(availability);
        }

        // GET: api/DoctorAvailability/{doctorId}
        [HttpGet("{doctorId:guid}")]
        public async Task<ActionResult<List<DoctorAvailabilityDto>>> GetByDoctorId(
            Guid doctorId)
        {
            var availability = await _service.GetByDoctorIdAsync(doctorId);

            return Ok(availability);
        }

        // POST: api/DoctorAvailability
        [HttpPost]
        public async Task<ActionResult<DoctorAvailabilityDto>> Create(
            CreateDoctorAvailabilityDto dto)
        {
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
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<DoctorAvailabilityDto>> Update(
            Guid id,
            UpdateDoctorAvailabilityDto dto)
        {
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
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _service.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound("Availability record not found.");
            }

            return NoContent();
        }
    }
}