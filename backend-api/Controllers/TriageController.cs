using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CareFlowAI.API.Data;
using CareFlowAI.API.Models;
using CareFlowAI.API.DTOs;
using CareFlowAI.API.Services;

namespace CareFlowAI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TriageController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly PlanningAgentService _planningAgent;

        public TriageController(ApplicationDbContext context, PlanningAgentService planningAgent)
        {
            _context       = context;
            _planningAgent = planningAgent;
        }

        // ────────────────────────────────────────────────────────────────────
        // POST api/triage
        // Flutter app calls this when a patient submits symptoms.
        // Saves the record, then immediately triggers the Planning Agent.
        // ────────────────────────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Submit([FromBody] CreateTriageRequestDto dto)
        {
            // 1. Validate that the patient exists
            var patient = await _context.PatientProfiles.FindAsync(dto.PatientId);
            if (patient == null)
                return NotFound("Patient profile not found.");

            // 2. Save the triage record
            var record = new TriageRecord
            {
                PatientId    = dto.PatientId,
                Symptoms     = dto.Symptoms,
                TriageStatus = "Pending"
            };
            _context.TriageRecords.Add(record);
            await _context.SaveChangesAsync();

            // 3. Run the Planning Agent (Gemini AI) and persist its workflow state
            var agentState = await _planningAgent.RunAsync(record);
            _context.AgentWorkflows.Add(agentState);

            // 4. Update severity on the triage record based on the agent's output
            if (agentState.AgentStatus == "Completed" && agentState.OutputPayload != null)
            {
                var plan = System.Text.Json.JsonSerializer.Deserialize<ClinicalPlan>(
                    agentState.OutputPayload,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }); // ClinicalPlan from ai-orchestrator

                if (plan != null)
                {
                    record.SeverityLevel = plan.UrgencyLevel;
                    record.TriageStatus  = "InReview";     // Ready for doctor review
                    record.UpdatedAt     = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = record.Id }, MapToDto(record, agentState));
        }

        // ────────────────────────────────────────────────────────────────────
        // GET api/triage/{id}
        // Returns a single triage record with its AI plan.
        // Used by both Flutter (patient status) and React (doctor review).
        // ────────────────────────────────────────────────────────────────────
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var record = await _context.TriageRecords
                .Include(t => t.AgentWorkflows)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (record == null)
                return NotFound();

            var agent = record.AgentWorkflows.FirstOrDefault(a => a.AgentName == "PlanningAgent");
            return Ok(MapToDto(record, agent));
        }

        // ────────────────────────────────────────────────────────────────────
        // GET api/triage
        // React dashboard: list all triage records awaiting doctor review.
        // ────────────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var records = await _context.TriageRecords
                .Include(t => t.AgentWorkflows)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            var response = records.Select(r =>
            {
                var agent = r.AgentWorkflows.FirstOrDefault(a => a.AgentName == "PlanningAgent");
                return MapToDto(r, agent);
            });

            return Ok(response);
        }

        // ────────────────────────────────────────────────────────────────────
        // PATCH api/triage/{id}/review
        // React dashboard: doctor approves or rejects the AI plan.
        // This is the human-in-the-loop step.
        // ────────────────────────────────────────────────────────────────────
        [HttpPatch("{id}/review")]
        public async Task<IActionResult> Review(Guid id, [FromBody] ReviewTriageDto dto)
        {
            if (dto.Decision != "Approved" && dto.Decision != "Rejected")
                return BadRequest("Decision must be 'Approved' or 'Rejected'.");

            var record = await _context.TriageRecords
                .Include(t => t.AgentWorkflows)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (record == null)
                return NotFound();

            // Update the triage record
            record.TriageStatus      = dto.Decision;
            record.DoctorNotes       = dto.DoctorNotes;
            record.AssignedDoctorId  = dto.AssignedDoctorId;
            record.UpdatedAt         = DateTime.UtcNow;

            // Update the agent's approval status
            var agent = record.AgentWorkflows.FirstOrDefault(a => a.AgentName == "PlanningAgent");
            if (agent != null)
            {
                agent.ApprovalStatus = dto.Decision;
                agent.UpdatedAt      = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return Ok(MapToDto(record, agent));
        }

        // ── Helper ───────────────────────────────────────────────────────────
        private static TriageResponseDto MapToDto(TriageRecord record, AgentWorkflowState? agent) =>
            new()
            {
                Id               = record.Id,
                PatientId        = record.PatientId,
                Symptoms         = record.Symptoms,
                SeverityLevel    = record.SeverityLevel,
                TriageStatus     = record.TriageStatus,
                DoctorNotes      = record.DoctorNotes,
                AssignedDoctorId = record.AssignedDoctorId,
                CreatedAt        = record.CreatedAt,
                UpdatedAt        = record.UpdatedAt,
                AiPlan           = agent?.OutputPayload,
                AiAgentStatus    = agent?.AgentStatus,
                ApprovalStatus   = agent?.ApprovalStatus
            };
    }
}
