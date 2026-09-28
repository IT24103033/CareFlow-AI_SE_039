using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CareFlowAI.API.Data;
using CareFlowAI.API.Models;
using CareFlowAI.API.DTOs;
using CareFlowAI.API.Services;

namespace CareFlowAI.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TriageController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly PlanningAgentService _planningAgent;
        private readonly ISafetyAgent _safetyAgent;

        public TriageController(
            ApplicationDbContext context,
            PlanningAgentService planningAgent,
            ISafetyAgent? safetyAgent = null)
        {
            _context       = context;
            _planningAgent = planningAgent;
            _safetyAgent   = safetyAgent ?? new PharmacyAiService();
        }

        // ────────────────────────────────────────────────────────────────────
        // POST api/triage
        // Flutter app calls this when a patient submits symptoms.
        // Saves the record, triggers the Planning Agent, and executes SafetyAgent validation.
        // ────────────────────────────────────────────────────────────────────
        [Authorize(Roles = "Patient,Doctor,Staff,Admin")]
        [HttpPost]
        public async Task<IActionResult> Submit([FromBody] CreateTriageRequestDto dto, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Symptoms) ||
                dto.Symptoms.Trim().Length < 20 || dto.Symptoms.Length > 4000 || dto.Duration?.Length > 200)
                return BadRequest("Provide symptoms between 20 and 4000 characters.");

            Guid patientId;
            if (User?.IsInRole("Patient") == true)
            {
                var patientClaim = User?.FindFirst("patient_id")?.Value;
                if (!Guid.TryParse(patientClaim, out var authPatientId))
                    return StatusCode(403, "Authenticated patient identity is missing or invalid.");

                if (dto.PatientId != Guid.Empty && dto.PatientId != authPatientId)
                    return StatusCode(403, "You cannot submit triage records on behalf of another patient.");

                patientId = authPatientId;
            }
            else
            {
                if (dto.PatientId == Guid.Empty)
                    return BadRequest("Provide a valid patient ID.");
                patientId = dto.PatientId;
            }

            // 1. Validate that the patient exists
            var patient = await _context.PatientProfiles.FindAsync(patientId);
            if (patient == null)
                return NotFound("Patient profile not found.");

            // 2. Save the triage record
            var record = new TriageRecord
            {
                PatientId    = patientId,
                Symptoms     = dto.Symptoms.Trim(),
                TriageStatus = "Pending",
                SeverityLevel = "Unassessed"
            };
            // Persist the record AND running workflow before any context/model call.
            var agentState = PlanningAgentService.CreateRunningState(record, dto.Duration);
            _context.TriageRecords.Add(record);
            _context.AgentWorkflows.Add(agentState);
            await _context.SaveChangesAsync(cancellationToken);

            await _planningAgent.RunAsync(record, agentState, cancellationToken);
            var plan = agentState.AgentStatus == "Completed" ? TriageReviewRules.ReadPlan(agentState.OutputPayload) : null;
            record.TriageStatus = plan == null ? "AssessmentFailed" : "InReview";
            record.SeverityLevel = plan?.UrgencyLevel ?? "Unassessed";

            // 3. Integrate Component D SafetyAgent: execute B's proposed emergency-safety step
            if (plan != null)
            {
                var safetyWorkflowState = _safetyAgent.CheckEmergencyRules(record, plan);
                _context.AgentWorkflows.Add(safetyWorkflowState);
            }

            record.UpdatedAt = DateTime.UtcNow;
            // Persist cancellation/failure even if the HTTP client disconnected. A process
            // crash still leaves a durable Running row for the future recovery worker.
            await _context.SaveChangesAsync(CancellationToken.None);

            return CreatedAtAction(nameof(GetById), new { id = record.Id }, MapToDto(record, agentState));
        }

        // ────────────────────────────────────────────────────────────────────
        // GET api/triage/{id}
        // Returns a single triage record with its AI plan.
        // Used by both Flutter (patient status) and React (doctor review).
        // ────────────────────────────────────────────────────────────────────
        [Authorize(Roles = "Patient,Doctor,Staff,Admin")]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var record = await _context.TriageRecords
                .Include(t => t.AgentWorkflows)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (record == null)
                return NotFound();

            if (User?.IsInRole("Patient") == true)
            {
                var patientClaim = User?.FindFirst("patient_id")?.Value;
                if (!Guid.TryParse(patientClaim, out var authPatientId) || record.PatientId != authPatientId)
                    return StatusCode(403, "You do not have permission to view this triage record.");
            }

            var agent = LatestAgent(record);
            return Ok(MapToDto(record, agent));
        }

        // ────────────────────────────────────────────────────────────────────
        // GET api/triage
        // React dashboard / Flutter history: list triage records.
        // Patients only see their own records; staff/doctors see all.
        // ────────────────────────────────────────────────────────────────────
        [Authorize(Roles = "Patient,Doctor,Staff,Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var query = _context.TriageRecords.AsNoTracking().Include(t => t.AgentWorkflows).AsQueryable();

            if (User?.IsInRole("Patient") == true)
            {
                var patientClaim = User?.FindFirst("patient_id")?.Value;
                if (!Guid.TryParse(patientClaim, out var authPatientId))
                    return StatusCode(403, "Authenticated patient identity is missing or invalid.");

                query = query.Where(t => t.PatientId == authPatientId);
            }

            var records = await query
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            var response = records.Select(r =>
            {
                var agent = LatestAgent(r);
                return MapToDto(r, agent);
            });

            return Ok(response);
        }

        // Separate staff endpoints preserve the existing mobile list response contract.
        [Authorize(Roles = "Doctor")]
        [HttpGet("review-queue")]
        public async Task<IActionResult> ReviewQueue([FromQuery] TriageQueueQuery filter)
        {
            var access = CheckStaffAccess();
            if (access != null) return access;
            var query = _context.TriageRecords.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.Trim();
                query = query.Where(t => t.Patient.FullName.Contains(search) || t.Symptoms.Contains(search));
            }
            if (filter.Status != null) query = query.Where(t => t.TriageStatus == filter.Status);
            if (filter.Severity != null) query = query.Where(t => t.SeverityLevel == filter.Severity);
            var total = await query.CountAsync();
            var sorted = filter.Sort switch
            {
                "oldest" => query.OrderBy(t => t.CreatedAt),
                "urgency" => query.OrderBy(t => t.SeverityLevel == "Critical" ? 0 :
                    t.SeverityLevel == "High" ? 1 : t.SeverityLevel == "Medium" ? 2 : 3).ThenBy(t => t.CreatedAt),
                _ => query.OrderByDescending(t => t.CreatedAt)
            };
            var records = await sorted.ThenBy(t => t.Id)
                .Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize)
                .Include(t => t.Patient).Include(t => t.AgentWorkflows).ToListAsync();
            return Ok(new { items = records.Select(r => MapToDto(r, LatestAgent(r))), total, filter.Page, filter.PageSize });
        }

        [Authorize(Roles = "Doctor")]
        [HttpGet("review-queue/{id:guid}")]
        public async Task<IActionResult> ReviewDetails(Guid id)
        {
            var access = CheckStaffAccess();
            if (access != null) return access;
            var record = await _context.TriageRecords.AsNoTracking().Include(t => t.Patient)
                .Include(t => t.AgentWorkflows).FirstOrDefaultAsync(t => t.Id == id);
            return record == null ? NotFound() : Ok(MapToDto(record, LatestAgent(record)));
        }

        [Authorize(Roles = "Doctor")]
        [HttpPatch("{id:guid}/review")]
        public async Task<IActionResult> Review(Guid id, [FromBody] ReviewTriageDto dto)
        {
            var access = CheckStaffAccess();
            if (access != null) return access;
            if (!Guid.TryParse(User?.FindFirst("doctor_id")?.Value, out var doctorId) ||
                !await _context.Doctors.AnyAsync(d => d.Id == doctorId && d.IsActive))
                return StatusCode(403, "An active doctor profile is required to review cases.");
            if (!new[] { "Approved", "Rejected", "RevisionRequested" }.Contains(dto.Decision))
                return BadRequest("Choose Approved, Rejected or RevisionRequested.");
            if (dto.ExpectedUpdatedAt == null) return BadRequest("The case version is required.");
            if (dto.DoctorNotes?.Length > 2000) return BadRequest("Notes must be at most 2000 characters.");
            if (dto.Decision != "Approved" && string.IsNullOrWhiteSpace(dto.DoctorNotes))
                return BadRequest("Provide a reason for rejection or revision.");

            var record = await _context.TriageRecords.Include(t => t.Patient)
                .Include(t => t.AgentWorkflows).FirstOrDefaultAsync(t => t.Id == id);
            if (record == null) return NotFound();
            var agent = LatestAgent(record);
            var conflict = TriageReviewRules.Check(record, agent, dto.ExpectedUpdatedAt);
            if (conflict != null) return Conflict(conflict);

            record.TriageStatus = dto.Decision;
            record.DoctorNotes = dto.DoctorNotes?.Trim();
            record.AssignedDoctorId = doctorId;
            record.UpdatedAt = DateTime.UtcNow;
            agent!.ApprovalStatus = dto.Decision;
            agent.UpdatedAt = record.UpdatedAt;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("Another reviewer changed this case. Refresh before continuing.");
            }
            return Ok(MapToDto(record, agent));
        }

        private IActionResult? CheckStaffAccess()
        {
            // Fail closed until Component A installs its authentication middleware.
            if (User?.Identity?.IsAuthenticated != true) return Unauthorized("Staff sign-in is required.");
            return (User?.IsInRole("Doctor") == true) ? null : StatusCode(403, "Doctor access is required.");
        }

        private static AgentWorkflowState? LatestAgent(TriageRecord record) => record.AgentWorkflows
            .Where(a => a.AgentName == "PlanningAgent")
            .OrderByDescending(a => a.CreatedAt).ThenByDescending(a => a.Id).FirstOrDefault();

        // ── Helper ───────────────────────────────────────────────────────────
        private static TriageResponseDto MapToDto(TriageRecord record, AgentWorkflowState? agent) =>
            new()
            {
                Id               = record.Id,
                PatientId        = record.PatientId,
                PatientName      = record.Patient?.FullName,
                Symptoms         = record.Symptoms,
                SeverityLevel    = record.SeverityLevel,
                TriageStatus     = record.TriageStatus,
                DoctorNotes      = record.DoctorNotes,
                AssignedDoctorId = record.AssignedDoctorId,
                CreatedAt        = record.CreatedAt,
                UpdatedAt        = record.UpdatedAt,
                AiPlan           = TriageReviewRules.ReadPlan(agent?.OutputPayload) == null ? null : agent?.OutputPayload,
                AiAgentStatus    = agent?.AgentStatus,
                ApprovalStatus   = agent?.ApprovalStatus,
                AnalysisMethod   = TriageReviewRules.ReadPlan(agent?.OutputPayload)?.AnalysisMethod,
                PlanningExecution = TriageReviewRules.ReadExecution(agent?.OutputPayload)
            };
    }
}
