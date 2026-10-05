using Microsoft.AspNetCore.Authorization;
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
        [Authorize(Roles = "Doctor,Staff,Admin")]
        [HttpGet]
        public async Task<IActionResult> GetPatients()
        {
            var patients = await _context.PatientProfiles.Where(p => p.IsActive).ToListAsync();
            return Ok(patients);
        }

        // GET: api/patientprofiles/{id}
        [Authorize(Roles = "Patient,Doctor,Staff,Admin")]
        [Authorize(Roles = "Patient,Doctor,Staff,Admin")]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var patientClaim = User?.FindFirst("patient_id")?.Value;
            if (!Guid.TryParse(patientClaim, out var authPatientId))
            {
                var fullName = User?.FindFirst("full_name")?.Value;
                if (string.IsNullOrEmpty(fullName)) return Unauthorized();
                var profile = await _context.PatientProfiles
                    .Include(p => p.Admissions).ThenInclude(a => a.Ward)
                    .FirstOrDefaultAsync(p => p.FullName == fullName);
                if (profile == null) return NotFound("Profile not found.");
                return Ok(profile);
            }
            var patient = await _context.PatientProfiles
                .Include(p => p.Admissions).ThenInclude(a => a.Ward)
                .FirstOrDefaultAsync(p => p.Id == authPatientId);
            if (patient == null) return NotFound();
            return Ok(patient);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            if (User?.IsInRole("Patient") == true)
            {
                var patientClaim = User?.FindFirst("patient_id")?.Value;
                if (!Guid.TryParse(patientClaim, out var authPatientId) || authPatientId != id)
                    return StatusCode(403, "You do not have permission to view this patient profile.");
            }

            var patient = await _context.PatientProfiles
                .Include(p => p.Admissions)
                    .ThenInclude(a => a.Ward)
                .FirstOrDefaultAsync(p => p.Id == id);
                
            if (patient == null || !patient.IsActive)
                return NotFound("Patient profile not found.");

            return Ok(patient);
        }

        // POST: api/patientprofiles
        [Authorize(Roles = "Doctor,Staff,Admin")]
        [HttpPost]
        public async Task<IActionResult> RegisterPatient([FromBody] PatientProfile patient)
        {
            _context.PatientProfiles.Add(patient);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = patient.Id }, patient);
        }

        // GET: api/patientprofiles/search?name=John
        [Authorize(Roles = "Doctor,Staff,Admin")]
        [HttpGet("search")]
        public async Task<IActionResult> SearchPatients([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                var all = await _context.PatientProfiles.Where(p => p.IsActive).ToListAsync();
                return Ok(all);
            }

            var nameLower = name.ToLower();
            var patients = await _context.PatientProfiles
                .Where(p => p.IsActive && p.FullName.ToLower().Contains(nameLower))
                .ToListAsync();

            return Ok(patients);
        }

        [Authorize(Roles = "Patient,Doctor,Staff,Admin")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdatePatient(Guid id, [FromBody] PatientProfileUpdateDto dto)
        {
            var patient = await _context.PatientProfiles.FindAsync(id);
            if (patient == null || !patient.IsActive)
                return NotFound("Patient not found.");

            if (User?.IsInRole("Patient") == true)
            {
                var patientClaim = User?.FindFirst("patient_id")?.Value;
                if (!Guid.TryParse(patientClaim, out var authPatientId) || authPatientId != id)
                {
                    // Fallback to name check if patient_id is missing
                    var fullNameClaim = User?.FindFirst("full_name")?.Value;
                    if (string.IsNullOrEmpty(fullNameClaim) || patient.FullName != fullNameClaim)
                    {
                        return StatusCode(403, "You do not have permission to update this patient profile.");
                    }
                }
            }

            if (dto.DateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow))
                return BadRequest("Date of birth cannot be in the future.");

            patient.FullName = dto.FullName.Trim();
            patient.DateOfBirth = dto.DateOfBirth;
            patient.BloodGroup = dto.BloodGroup.Trim();
            patient.MedicalHistorySummary = (dto.MedicalHistorySummary ?? string.Empty).Trim();

            await _context.SaveChangesAsync();
            return Ok(patient);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeletePatient(Guid id)
        {
            var patient = await _context.PatientProfiles
                .Include(p => p.Admissions)
                    .ThenInclude(a => a.Ward)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null || !patient.IsActive)
                return NotFound("Patient not found.");

            // Discharge the patient from any active admissions so beds aren't permanently locked
            foreach (var admission in patient.Admissions)
            {
                if (admission.DischargedAt == null && admission.Ward != null)
                {
                    if (admission.Ward.OccupiedBeds > 0)
                        admission.Ward.OccupiedBeds--;
                    admission.DischargedAt = DateTime.UtcNow;
                }
            }

            // Perform the Soft Delete
            patient.IsActive = false;
            
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
