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
            var patients = await _context.PatientProfiles
                .Where(p => p.FullName.Contains(name))
                .ToListAsync();
            
            return Ok(patients);
        }
    }
}