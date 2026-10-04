using CareFlowAI.API.Data;
using CareFlowAI.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareFlowAI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WardsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public WardsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetWards()
        {
            var wards = await _context.Wards.AsNoTracking().OrderBy(w => w.WardNumber).ToListAsync();
            return Ok(wards);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateWard([FromBody] WardUpsertDto dto)
        {
            var number = dto.WardNumber.Trim();
            if (await _context.Wards.AnyAsync(w => EF.Functions.ILike(w.WardNumber, number)))
                return Conflict("A ward with that number already exists.");

            var ward = new Ward
            {
                WardNumber = number,
                WardType = dto.WardType.Trim(),
                Capacity = dto.Capacity,
                OccupiedBeds = 0
            };

            _context.Wards.Add(ward);
            await _context.SaveChangesAsync();
            return Ok(ward);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateWard(Guid id, [FromBody] WardUpsertDto dto)
        {
            var ward = await _context.Wards.FindAsync(id);
            if (ward == null)
                return NotFound("Ward not found.");

            if (dto.Capacity < ward.OccupiedBeds)
                return BadRequest($"Capacity cannot be below current occupancy ({ward.OccupiedBeds}).");

            var number = dto.WardNumber.Trim();
            if (await _context.Wards.AnyAsync(w => w.Id != id && EF.Functions.ILike(w.WardNumber, number)))
                return Conflict("A ward with that number already exists.");

            ward.WardNumber = number;
            ward.WardType = dto.WardType.Trim();
            ward.Capacity = dto.Capacity;
            await _context.SaveChangesAsync();
            return Ok(ward);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteWard(Guid id)
        {
            var ward = await _context.Wards.FindAsync(id);
            if (ward == null)
                return NotFound("Ward not found.");

            if (ward.OccupiedBeds > 0)
                return BadRequest("Cannot delete a ward that still has occupied beds. Discharge patients first.");

            var hasAdmissions = await _context.Admissions.AnyAsync(a => a.WardId == id);
            if (hasAdmissions)
            {
                _context.Admissions.RemoveRange(_context.Admissions.Where(a => a.WardId == id));
            }

            _context.Wards.Remove(ward);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
