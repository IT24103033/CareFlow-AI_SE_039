using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CareFlowAI.API.Data;
using CareFlowAI.API.Models;

namespace CareFlowAI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientProfilesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PatientProfilesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/patientprofiles
        [HttpGet]
        public async Task<IActionResult> GetPatients()
        {
            var patients = await _context.PatientProfiles.ToListAsync();
            return Ok(patients);
        }

        // POST: api/patientprofiles
        [HttpPost]
        public async Task<IActionResult> RegisterPatient([FromBody] PatientProfile patient)
        {
            _context.PatientProfiles.Add(patient);
            await _context.SaveChangesAsync();
            return Ok(patient);
        }

        // GET: api/patientprofiles/search?name=John
        [HttpGet("search")]
        public async Task<IActionResult> SearchPatients([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                var all = await _context.PatientProfiles.ToListAsync();
                return Ok(all);
            }

            // ILike = case-insensitive LIKE on PostgreSQL
            var patients = await _context.PatientProfiles
                .Where(p => EF.Functions.ILike(p.FullName, $"%{name}%"))
                .ToListAsync();

            return Ok(patients);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetPatient(Guid id)
        {
            var patient = await _context.PatientProfiles.FindAsync(id);
            if (patient == null)
                return NotFound("Patient not found.");

            return Ok(patient);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdatePatient(Guid id, [FromBody] PatientProfileUpdateDto dto)
        {
            var patient = await _context.PatientProfiles.FindAsync(id);
            if (patient == null)
                return NotFound("Patient not found.");

            if (dto.DateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow))
                return BadRequest("Date of birth cannot be in the future.");

            patient.FullName = dto.FullName.Trim();
            patient.DateOfBirth = dto.DateOfBirth;
            patient.BloodGroup = dto.BloodGroup.Trim();
            patient.MedicalHistorySummary = (dto.MedicalHistorySummary ?? string.Empty).Trim();

            await _context.SaveChangesAsync();
            return Ok(patient);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeletePatient(Guid id)
        {
            var patient = await _context.PatientProfiles
                .Include(p => p.Admissions)
                    .ThenInclude(a => a.Ward)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null)
                return NotFound("Patient not found.");

            foreach (var admission in patient.Admissions)
            {
                if (admission.DischargedAt == null && admission.Ward != null && admission.Ward.OccupiedBeds > 0)
                    admission.Ward.OccupiedBeds--;
            }

            _context.Admissions.RemoveRange(patient.Admissions);
            _context.PatientProfiles.Remove(patient);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}