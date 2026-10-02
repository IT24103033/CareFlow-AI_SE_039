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
        private readonly CloudinaryDotNet.Cloudinary? _cloudinary;
        private readonly CareFlowAI.Orchestrator.Agents.AppointmentActionAgent _appointmentAgent;

        public TriageController(
            ApplicationDbContext context,
            PlanningAgentService planningAgent,
            IServiceProvider serviceProvider,
            CareFlowAI.Orchestrator.Agents.AppointmentActionAgent appointmentAgent,
            ISafetyAgent? safetyAgent = null)
        {
            _context       = context;
            _planningAgent = planningAgent;
            _appointmentAgent = appointmentAgent;
            _safetyAgent   = safetyAgent ?? new PharmacyAiService();
            _cloudinary    = serviceProvider.GetService<CloudinaryDotNet.Cloudinary>();
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
                AttachmentId = dto.AttachmentId,
                TriageStatus = "Pending",
                SeverityLevel = "Unassessed"
            };

            if (dto.AttachmentId.HasValue)
            {
                var attachment = await _context.TriageAttachments.FindAsync(dto.AttachmentId.Value);
                if (attachment != null && attachment.UploaderId == patientId && attachment.TriageRecordId == null)
                {
                    attachment.TriageRecordId = record.Id;
                    record.Attachment = attachment;
                }
                else
                {
                    return BadRequest("Invalid attachment ID or unauthorized.");
                }
            }
            // Persist the record AND running workflow before any context/model call.
            var agentState = PlanningAgentService.CreateRunningState(record, dto.Duration);
            _context.TriageRecords.Add(record);
            _context.AgentWorkflows.Add(agentState);
            await _context.SaveChangesAsync(cancellationToken);

            await _planningAgent.RunAsync(record, agentState, cancellationToken);
            var plan = agentState.AgentStatus == "Completed" ? TriageReviewRules.ReadPlan(agentState.OutputPayload) : null;
            record.TriageStatus = plan == null ? "AssessmentFailed" : "InReview";
            record.SeverityLevel = plan?.UrgencyLevel ?? "Unassessed";

            // 3. Integrate Component C ActionAgent and Component D SafetyAgent
            if (plan != null)
            {
                await TriageOrchestrationHelper.ExecuteDownstreamActionsAsync(
                    record, plan, _context, _safetyAgent, _appointmentAgent);
            }

            record.UpdatedAt = DateTime.UtcNow;
            // Persist cancellation/failure even if the HTTP client disconnected. A process
            // crash still leaves a durable Running row for the future recovery worker.
            await _context.SaveChangesAsync(CancellationToken.None);

            return CreatedAtAction(nameof(GetById), new { id = record.Id }, MapToDto(record, agentState, _cloudinary));
        }

        [Authorize(Roles = "Patient,Doctor,Admin")]
        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage(IFormFile image, [FromServices] CloudinaryDotNet.Cloudinary cloudinary, CancellationToken cancellationToken = default)
        {
            if (image == null) return BadRequest("No image provided (image parameter is null).");
            if (image.Length == 0) return BadRequest("Image provided but length is 0.");
            
            if (image.Length > 5 * 1024 * 1024) return BadRequest($"Image size ({image.Length} bytes) exceeds the 5MB limit.");

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "application/octet-stream" };
            if (!allowedTypes.Contains(image.ContentType)) return BadRequest($"Invalid file type '{image.ContentType}'. Only JPEG, PNG, and WebP are allowed.");

            if (cloudinary == null) return StatusCode(500, "Cloudinary configuration is missing.");

            using var stream = image.OpenReadStream();
            var uploadParams = new CloudinaryDotNet.Actions.ImageUploadParams
            {
                File = new CloudinaryDotNet.FileDescription(image.FileName, stream),
                Folder = "triage-images",
                Type = "authenticated"
            };

            var uploadResult = await cloudinary.UploadAsync(uploadParams, cancellationToken);
            
            if (uploadResult.Error != null) return StatusCode(500, $"Image upload failed: {uploadResult.Error.Message}");

            Guid uploaderId = Guid.Empty;
            if (User?.IsInRole("Patient") == true)
            {
                var patientClaim = User?.FindFirst("patient_id")?.Value;
                Guid.TryParse(patientClaim, out uploaderId);
            }
            else
            {
                var doctorClaim = User?.FindFirst("doctor_id")?.Value;
                Guid.TryParse(doctorClaim, out uploaderId);
            }

            if (uploaderId == Guid.Empty) return Unauthorized("Invalid identity.");

            var attachment = new TriageAttachment
            {
                CloudinaryPublicId = uploadResult.PublicId,
                UploaderId = uploaderId
            };
            _context.TriageAttachments.Add(attachment);
            await _context.SaveChangesAsync(cancellationToken);

            return Ok(new { attachmentId = attachment.Id });
        }

        // ────────────────────────────────────────────────────────────────────
        // GET api/triage/{id}/image
        // Returns the signed Cloudinary URL for the triage record's image.
        // Enforces patient ownership and staff authorization.
        // ────────────────────────────────────────────────────────────────────
        [Authorize(Roles = "Patient,Doctor,Staff,Admin")]
        [HttpGet("{id:guid}/image")]
        public async Task<IActionResult> GetImage(Guid id, [FromServices] CloudinaryDotNet.Cloudinary cloudinary)
        {
            var record = await _context.TriageRecords.Include(t => t.Attachment).FirstOrDefaultAsync(t => t.Id == id);
            if (record == null || record.Attachment == null)
                return NotFound();

            if (User?.IsInRole("Patient") == true)
            {
                var patientClaim = User?.FindFirst("patient_id")?.Value;
                if (!Guid.TryParse(patientClaim, out var authPatientId) || record.PatientId != authPatientId)
                    return StatusCode(403, "You do not have permission to view this image.");
            }

            var publicId = record.Attachment.CloudinaryPublicId;

            var transform = new CloudinaryDotNet.Transformation();
            var signedUrl = cloudinary.Api.UrlImgUp.Transform(transform)
                .Action("authenticated")
                .Signed(true)
                .BuildUrl(publicId);

            return Redirect(signedUrl);
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
                .Include(t => t.AgentWorkflows).Include(t => t.Attachment)
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
            return Ok(MapToDto(record, agent, _cloudinary));
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
            var query = _context.TriageRecords.AsNoTracking().Include(t => t.AgentWorkflows).Include(t => t.Attachment).AsQueryable();

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
                return MapToDto(r, agent, _cloudinary);
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
                .Include(t => t.Patient).Include(t => t.AgentWorkflows).Include(t => t.Attachment).ToListAsync();
            return Ok(new { items = records.Select(r => MapToDto(r, LatestAgent(r), _cloudinary)), total, filter.Page, filter.PageSize });
        }

        [Authorize(Roles = "Doctor")]
        [HttpGet("review-queue/{id:guid}")]
        public async Task<IActionResult> ReviewDetails(Guid id)
        {
            var access = CheckStaffAccess();
            if (access != null) return access;
            var record = await _context.TriageRecords.AsNoTracking().Include(t => t.Patient)
                .Include(t => t.AgentWorkflows).Include(t => t.Attachment).Include(t => t.ReviewHistories).FirstOrDefaultAsync(t => t.Id == id);
            return record == null ? NotFound() : Ok(MapToDto(record, LatestAgent(record), _cloudinary));
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
                .Include(t => t.AgentWorkflows).Include(t => t.Attachment).FirstOrDefaultAsync(t => t.Id == id);
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

            var history = new TriageReviewHistory
            {
                TriageRecordId = record.Id,
                AssignedDoctorId = doctorId,
                Action = dto.Decision,
                Notes = dto.DoctorNotes?.Trim(),
                SymptomsAtReview = record.Symptoms,
                AgentWorkflowStateId = agent?.Id
            };
            _context.TriageReviewHistories.Add(history);

            if (record.TentativeAppointmentId.HasValue)
            {
                var appointmentService = HttpContext.RequestServices.GetRequiredService<AppointmentService>();
                if (dto.Decision == "Approved")
                {
                    await appointmentService.ConfirmAsync(record.TentativeAppointmentId.Value);
                }
                else if (dto.Decision == "Rejected" || dto.Decision == "RevisionRequested")
                {
                    await appointmentService.CancelAsync(record.TentativeAppointmentId.Value);
                    record.TentativeAppointmentId = null;
                }
            }

            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("Another reviewer changed this case. Refresh before continuing.");
            }
            return Ok(MapToDto(record, agent, _cloudinary));
        }

        [Authorize(Roles = "Patient")]
        [HttpPost("{id:guid}/revise")]
        public async Task<IActionResult> Revise(Guid id, [FromBody] PatientRevisionDto dto, CancellationToken cancellationToken = default)
        {
            var patientClaim = User?.FindFirst("patient_id")?.Value;
            if (!Guid.TryParse(patientClaim, out var authPatientId))
                return StatusCode(403, "Authenticated patient identity is missing or invalid.");

            var record = await _context.TriageRecords
                .Include(t => t.AgentWorkflows).Include(t => t.Attachment)
                .FirstOrDefaultAsync(t => t.Id == id);
            
            if (record == null) return NotFound();
            
            if (record.PatientId != authPatientId)
                return StatusCode(403, "You can only revise your own triage records.");

            if (record.TriageStatus != "RevisionRequested")
                return BadRequest("This record is not currently requesting a revision.");

            if (record.UpdatedAt != dto.ExpectedUpdatedAt)
                return Conflict("This case has changed. Please refresh.");

            record.Symptoms = dto.UpdatedSymptoms.Trim();
            record.TriageStatus = "ReassessmentInProgress";
            record.UpdatedAt = DateTime.UtcNow;

            // Trigger reassessment - persist state first
            var agentState = PlanningAgentService.CreateRunningState(record, null);
            _context.AgentWorkflows.Add(agentState);
            
            try { await _context.SaveChangesAsync(CancellationToken.None); }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("Another update occurred. Please refresh.");
            }

            await _planningAgent.RunAsync(record, agentState, cancellationToken);
            
            var plan = agentState.AgentStatus == "Completed" ? TriageReviewRules.ReadPlan(agentState.OutputPayload) : null;
            record.TriageStatus = plan == null ? "AssessmentFailed" : "InReview";
            record.SeverityLevel = plan?.UrgencyLevel ?? record.SeverityLevel;
            record.UpdatedAt = DateTime.UtcNow;

            if (plan != null)
            {
                await TriageOrchestrationHelper.ExecuteDownstreamActionsAsync(
                    record, plan, _context, _safetyAgent, _appointmentAgent);
            }

            await _context.SaveChangesAsync(CancellationToken.None);

            return Ok(MapToDto(record, agentState, _cloudinary));
        }

        [Authorize(Roles = "Patient,Doctor,Admin")]
        [HttpPost("{id:guid}/retry")]
        public async Task<IActionResult> Retry(Guid id, CancellationToken cancellationToken = default)
        {
            var record = await _context.TriageRecords
                .Include(t => t.AgentWorkflows).Include(t => t.Attachment)
                .FirstOrDefaultAsync(t => t.Id == id);
            
            if (record == null) return NotFound();
            
            if (User?.IsInRole("Patient") == true)
            {
                var patientClaim = User?.FindFirst("patient_id")?.Value;
                if (!Guid.TryParse(patientClaim, out var authPatientId) || record.PatientId != authPatientId)
                    return StatusCode(403, "You can only retry your own triage records.");
            }

            if (record.TriageStatus != "AssessmentFailed")
                return BadRequest("Only failed assessments can be retried.");

            record.TriageStatus = "ReassessmentInProgress";
            record.UpdatedAt = DateTime.UtcNow;

            var agentState = PlanningAgentService.CreateRunningState(record, null);
            _context.AgentWorkflows.Add(agentState);
            
            try { await _context.SaveChangesAsync(CancellationToken.None); }
            catch (DbUpdateConcurrencyException) { return Conflict("Another update occurred. Please refresh."); }

            await _planningAgent.RunAsync(record, agentState, cancellationToken);
            
            var plan = agentState.AgentStatus == "Completed" ? TriageReviewRules.ReadPlan(agentState.OutputPayload) : null;
            record.TriageStatus = plan == null ? "AssessmentFailed" : "InReview";
            record.SeverityLevel = plan?.UrgencyLevel ?? record.SeverityLevel;
            record.UpdatedAt = DateTime.UtcNow;

            if (plan != null)
            {
                await TriageOrchestrationHelper.ExecuteDownstreamActionsAsync(
                    record, plan, _context, _safetyAgent, _appointmentAgent);
            }

            await _context.SaveChangesAsync(CancellationToken.None);

            return Ok(MapToDto(record, agentState, _cloudinary));
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
        private static TriageResponseDto MapToDto(TriageRecord record, AgentWorkflowState? agent, CloudinaryDotNet.Cloudinary? cloudinary = null)
        {
            string? imageUrl = null;
            if (record.Attachment != null && cloudinary != null)
            {
                try
                {
                    var publicId = record.Attachment.CloudinaryPublicId;
                    var transform = new CloudinaryDotNet.Transformation();
                    imageUrl = cloudinary.Api.UrlImgUp.Transform(transform)
                        .Action("authenticated")
                        .Signed(true)
                        .BuildUrl(publicId);
                } catch { /* fallback */ }
            }

            return new TriageResponseDto
            {
                Id               = record.Id,
                PatientId        = record.PatientId,
                PatientName      = record.Patient?.FullName,
                Symptoms         = record.Symptoms,
                ImageUrl         = imageUrl,
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
                PlanningExecution = TriageReviewRules.ReadExecution(agent?.OutputPayload),
                TentativeAppointmentId = record.TentativeAppointmentId,
                ReviewHistories  = record.ReviewHistories?.OrderByDescending(h => h.CreatedAt).Select(h => new TriageReviewHistoryDto
                {
                    Id = h.Id,
                    ReviewerId = h.AssignedDoctorId,
                    LinkedAttemptId = h.AgentWorkflowStateId,
                    Action = h.Action,
                    Notes = h.Notes,
                    SymptomsAtReview = h.SymptomsAtReview,
                    PreviousPlan = h.AgentWorkflowState?.OutputPayload,
                    CreatedAt = h.CreatedAt
                }).ToList() ?? new List<TriageReviewHistoryDto>()
            };
        }
    }
}
