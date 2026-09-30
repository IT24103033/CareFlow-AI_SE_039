using Microsoft.AspNetCore.Authorization;
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
        private readonly CareFlowAI.API.Services.IPatientContextTool _contextTool;
        private readonly CareFlowAI.Orchestrator.Agents.IDomainAnalysisAgent _domainAnalysisAgent;

        public AdmissionsController(
            ApplicationDbContext context, 
            IConfiguration configuration, 
            CareFlowAI.API.Services.IPatientContextTool contextTool,
            CareFlowAI.Orchestrator.Agents.IDomainAnalysisAgent domainAnalysisAgent)
        {
            _context = context;
            _configuration = configuration;
            _contextTool = contextTool;
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
            if (request.PatientId == Guid.Empty)
                return BadRequest("PatientId is required.");

            var patientExists = await _context.PatientProfiles.AnyAsync(p => p.Id == request.PatientId, cancellationToken);
            if (!patientExists)
                return NotFound("Patient not found.");

            try 
            {
                var analysisResult = await _domainAnalysisAgent.AnalyzeRiskAsync(request, cancellationToken);
                return Ok(analysisResult);
            }
            catch (CareFlowAI.Orchestrator.Agents.DomainAnalysisException ex)
            {
                if (ex.ErrorCode == "DOMAIN_PROVIDER_UNAVAILABLE" || ex.ErrorCode == "DOMAIN_NOT_CONFIGURED")
                    return StatusCode(503, new { error = "AI provider is temporarily unavailable or misconfigured." });
                if (ex.ErrorCode == "DOMAIN_INVALID_OUTPUT")
                    return StatusCode(422, new { error = "AI provider returned invalid or unparseable clinical output." });
                
                return StatusCode(500, new { error = "An internal AI analysis error occurred." });
            }
            catch (OperationCanceledException)
            {
                return StatusCode(408, new { error = "Analysis timed out or was cancelled." });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "An unexpected error occurred during analysis." });
            }
        }
    }
}