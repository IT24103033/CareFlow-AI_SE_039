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
            var patients = await _context.PatientProfiles.ToListAsync();
            return Ok(patients);
        }

        // GET: api/patientprofiles/{id}
        [Authorize(Roles = "Patient,Doctor,Staff,Admin")]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            if (User?.IsInRole("Patient") == true)
            {
                var patientClaim = User?.FindFirst("patient_id")?.Value;
                if (!Guid.TryParse(patientClaim, out var authPatientId) || authPatientId != id)
                    return StatusCode(403, "You do not have permission to view this patient profile.");
            }

            var patient = await _context.PatientProfiles.FindAsync(id);
            if (patient == null)
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
                var all = await _context.PatientProfiles.ToListAsync();
                return Ok(all);
            }

            var nameLower = name.ToLower();
            var patients = await _context.PatientProfiles
                .Where(p => p.FullName.ToLower().Contains(nameLower))
                .ToListAsync();

            return Ok(patients);
        }
    }
}