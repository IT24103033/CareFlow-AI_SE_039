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
        private readonly IPatientHistoryTool _patientHistoryTool;

        public AdmissionsController(
            ApplicationDbContext context,
            IConfiguration configuration,
            IPatientHistoryTool patientHistoryTool)
        {
            _context = context;
            _configuration = configuration;
            _patientHistoryTool = patientHistoryTool;
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
            // Read the secure key from appsettings
            string apiKey = _configuration["Gemini:ApiKey"]
                ?? _configuration["GeminiApiKey"]
                ?? string.Empty;
            string model = _configuration["Gemini:Model"] ?? "gemini-flash-latest";
            var agent = new DomainAnalysisAgent(apiKey, _patientHistoryTool, model);
            
            var analysisResult = await agent.AnalyzeRiskAsync(request);
            return Ok(analysisResult);
        }
    }
}