using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CareFlowAI.API.Data;
using CareFlowAI.API.Models;

namespace CareFlowAI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdmissionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AdmissionsController(ApplicationDbContext context,IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // POST: api/admissions/allocate-ward
        [HttpPost("allocate-ward")]
        public async Task<IActionResult> AllocateWard([FromBody] AdmissionRequestDto request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var ward = await _context.Wards.FindAsync(request.WardId);

                if (ward == null)
                    return NotFound("Ward not found.");

                if (ward.OccupiedBeds >= ward.Capacity)
                    return BadRequest("Ward is full. Cannot allocate bed.");

                // Build the Admission entity from the DTO
                var admission = new Admission
                {
                    PatientProfileId = request.PatientProfileId,
                    WardId = request.WardId
                };

                // Reserve the bed
                ward.OccupiedBeds++;
                _context.Admissions.Add(admission);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(admission);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"An error occurred during ward allocation: {ex.Message}");
            }
        }

        // POST: api/admissions/analyze-risk
        [HttpPost("analyze-risk")]
        public async Task<IActionResult> AnalyzePatientRisk([FromBody] CareFlowAI.Orchestrator.Agents.AgentInput request)
        {
            // Read the secure key from appsettings
            string apiKey = _configuration["GeminiApiKey"];
            
            // Pass the key into the agent
            var agent = new CareFlowAI.Orchestrator.Agents.DomainAnalysisAgent(apiKey);
            
            var analysisResult = await agent.AnalyzeRiskAsync(request);
            return Ok(analysisResult);
        }
    }
}