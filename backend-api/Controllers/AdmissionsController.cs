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
        public async Task<IActionResult> AllocateWard([FromBody] Admission request)
        {
            // We use a transaction to safely update both the Ward and Admission at the same time
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var ward = await _context.Wards.FindAsync(request.WardId);
                
                if (ward == null) 
                    return NotFound("Ward not found.");
                    
                if (ward.OccupiedBeds >= ward.Capacity) 
                    return BadRequest("Ward is full. Cannot allocate bed.");

                // Reserve the bed
                ward.OccupiedBeds++;
                _context.Admissions.Add(request);
                
                await _context.SaveChangesAsync();
                await transaction.CommitAsync(); // Lock in the changes

                return Ok(request);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(); // Cancel changes if an error happens
                return StatusCode(500, "An error occurred during ward allocation.");
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