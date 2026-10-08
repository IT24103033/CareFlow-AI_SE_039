using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CareFlowAI.API.Data;
using CareFlowAI.API.Models;
using CareFlowAI.Orchestrator.Agents;
using CareFlowAI.Orchestrator.Tools;

namespace CareFlowAI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdmissionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IDomainAnalysisAgent _domainAnalysisAgent;

        public AdmissionsController(
            ApplicationDbContext context,
            IConfiguration configuration,
            IDomainAnalysisAgent domainAnalysisAgent)
        {
            _context = context;
            _configuration = configuration;
            _domainAnalysisAgent = domainAnalysisAgent;
        }

        // POST: api/admissions/allocate-ward
        [Authorize(Roles = "Staff,Admin")]
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

        [Authorize(Roles = "Doctor,Staff,Admin")]
        [HttpPost("analyze-risk")]
        public async Task<IActionResult> AnalyzePatientRisk([FromBody] CareFlowAI.Orchestrator.Agents.AgentInput request, CancellationToken cancellationToken)
        {   
            if (string.IsNullOrWhiteSpace(request.PatientName))
            {
                return BadRequest(new { error = "Patient name is strictly required." });
            }
            
            try
            {
                var analysisResult = await _domainAnalysisAgent.AnalyzeRiskAsync(request);
                return Ok(analysisResult);
            }
            catch (CareFlowAI.Orchestrator.Agents.DomainAnalysisException ex)
            {
                if (ex.ErrorCode == "DOMAIN_INVALID_OUTPUT")
                    return StatusCode(422, new { error = "AI provider returned invalid output." });
                
                return StatusCode(503, new { error = "AI provider is temporarily unavailable." });
            }
        }
    }
}