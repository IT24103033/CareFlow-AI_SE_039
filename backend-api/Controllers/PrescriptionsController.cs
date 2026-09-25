using System;
using System.Linq;
using System.Threading.Tasks;
using CareFlowAI.API.Data;
using CareFlowAI.API.DTOs;
using CareFlowAI.API.Models;
using CareFlowAI.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareFlowAI.API.Controllers
{
    /// <summary>
    /// Component D – Prescriptions API
    /// Manages e-prescription lifecycle: Draft → AI Safety Check → Doctor Approval → Dispensed.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PrescriptionsController : ControllerBase
    {
        private readonly ApplicationDbContext  _context;
        private readonly PharmacyAiService     _aiService;
        private readonly INotificationService  _notificationService;

        public PrescriptionsController(
            ApplicationDbContext context,
            PharmacyAiService aiService,
            INotificationService notificationService)
        {
            _context             = context;
            _aiService          = aiService;
            _notificationService = notificationService;
        }

        // ── GET /api/prescriptions ───────────────────────────────────────────
        /// <summary>
        /// List prescriptions with search, filter, sort and pagination.
        /// Query params:
        ///   patientId  - filter by patient GUID
        ///   status     - filter by status ("Draft"|"Issued"|"Dispensed"|"Cancelled")
        ///   aiStatus   - filter by AI verdict ("Safe"|"Warning"|"Blocked")
        ///   search     - text search on patient name
        ///   sortBy     - "created" (default) | "status" | "patient"
        ///   sortDir    - "desc" (default) | "asc"
        ///   page       - page number (default 1)
        ///   pageSize   - items per page (default 10, max 50)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] Guid?   patientId = null,
            [FromQuery] string? status    = null,
            [FromQuery] string? aiStatus  = null,
            [FromQuery] string? search    = null,
            [FromQuery] string  sortBy    = "created",
            [FromQuery] string  sortDir   = "desc",
            [FromQuery] int     page      = 1,
            [FromQuery] int     pageSize  = 10)
        {
            pageSize = Math.Clamp(pageSize, 1, 50);
            page     = Math.Max(1, page);

            var query = _context.Prescriptions
                .Include(p => p.Patient)
                .Include(p => p.TriageRecord)
                .Include(p => p.Items)
                    .ThenInclude(i => i.Medicine)
                .AsQueryable();

            // ── Filter ────────────────────────────────────────────────────────
            if (patientId.HasValue)
                query = query.Where(p => p.PatientId == patientId.Value);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(p => p.Status == status);

            if (!string.IsNullOrWhiteSpace(aiStatus))
                query = query.Where(p => p.AiSafetyStatus == aiStatus);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                query = query.Where(p => p.Patient.FullName.ToLower().Contains(s));
            }

            // ── Sort ──────────────────────────────────────────────────────────
            bool desc = sortDir.ToLower() != "asc";
            query = sortBy.ToLower() switch
            {
                "status"  => desc ? query.OrderByDescending(p => p.Status)            : query.OrderBy(p => p.Status),
                "patient" => desc ? query.OrderByDescending(p => p.Patient.FullName)  : query.OrderBy(p => p.Patient.FullName),
                _         => desc ? query.OrderByDescending(p => p.CreatedAt)         : query.OrderBy(p => p.CreatedAt),
            };

            // ── Paginate ──────────────────────────────────────────────────────
            int total = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                TotalCount = total,
                Page       = page,
                PageSize   = pageSize,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                Items      = items.Select(p => new
                {
                    p.Id,
                    PatientName      = p.Patient.FullName,
                    p.PatientId,
                    p.TriageRecordId,
                    TriageSeverity   = p.TriageRecord.SeverityLevel,
                    p.IssuedByDoctorId,
                    p.Status,
                    p.AiSafetyStatus,
                    p.Notes,
                    p.NotificationSent,
                    p.NotificationChannel,
                    p.NotifiedAt,
                    p.CreatedAt,
                    p.UpdatedAt,
                    Items = p.Items.Select(i => new
                    {
                        i.Id,
                        i.MedicineId,
                        MedicineName = i.Medicine.Name,
                        MedicineCategory = i.Medicine.Category,
                        i.Quantity,
                        i.Dosage,
                        i.DurationDays,
                        StockAvailable = i.Medicine.StockQuantity
                    })
                })
            });
        }

        // ── GET /api/prescriptions/{id} ──────────────────────────────────────
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var prescription = await _context.Prescriptions
                .Include(p => p.Patient)
                .Include(p => p.TriageRecord)
                .Include(p => p.Items)
                    .ThenInclude(i => i.Medicine)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (prescription is null)
                return NotFound(new { message = "Prescription not found." });

            return Ok(prescription);
        }

        // ── POST /api/prescriptions ──────────────────────────────────────────
        /// <summary>
        /// Creates a Draft prescription, then immediately runs the AI Validation/Safety Agent.
        /// The prescription is paused (stays "Draft") until a doctor approves via PATCH /approve.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePrescriptionDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Verify patient exists
            if (!await _context.PatientProfiles.AnyAsync(p => p.Id == dto.PatientId))
                return NotFound(new { message = $"Patient {dto.PatientId} not found." });

            // Verify triage record exists and get severity
            var triage = await _context.TriageRecords.FindAsync(dto.TriageRecordId);
            if (triage is null)
                return NotFound(new { message = $"Triage record {dto.TriageRecordId} not found." });

            // Verify all medicines exist and load them
            var medicineIds = dto.Items.Select(i => i.MedicineId).Distinct().ToList();
            var medicines = await _context.Medicines
                .Where(m => medicineIds.Contains(m.Id))
                .ToListAsync();

            if (medicines.Count != medicineIds.Count)
            {
                var missing = medicineIds.Except(medicines.Select(m => m.Id));
                return BadRequest(new { message = "One or more medicine IDs not found.", MissingIds = missing });
            }

            // Build the prescription entity
            var prescription = new Prescription
            {
                PatientId      = dto.PatientId,
                TriageRecordId = dto.TriageRecordId,
                Notes          = dto.Notes,
                Status         = "Draft"
            };

            // Build line items (used for AI check before saving)
            var prescriptionItems = dto.Items.Select(itemDto => new PrescriptionItem
            {
                MedicineId  = itemDto.MedicineId,
                Medicine    = medicines.First(m => m.Id == itemDto.MedicineId),
                Quantity    = itemDto.Quantity,
                Dosage      = itemDto.Dosage,
                DurationDays = itemDto.DurationDays
            }).ToList();

            // ── Run AI Validation/Safety Agent ────────────────────────────────
            var safetyResult = _aiService.RunSafetyCheck(prescriptionItems, triage.SeverityLevel);

            prescription.AiSafetyStatus     = safetyResult.Verdict;
            prescription.AiSafetyCheckResult = safetyResult.ResultJson;

            // Remove the in-memory medicine nav props so EF doesn't try to insert them
            foreach (var item in prescriptionItems)
                item.Medicine = null!;

            prescription.Items = prescriptionItems;

            _context.Prescriptions.Add(prescription);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = prescription.Id }, new
            {
                prescription.Id,
                prescription.Status,
                prescription.AiSafetyStatus,
                AiSafetyCheckResult = prescription.AiSafetyCheckResult,
                message = safetyResult.Verdict == "Blocked"
                    ? "Prescription created (Draft) but BLOCKED by AI Safety Agent. Doctor must resolve errors before issuing."
                    : safetyResult.Verdict == "Warning"
                        ? "Prescription created (Draft) with AI warnings. Doctor review required before issuing."
                        : "Prescription created (Draft). AI Safety check passed. Awaiting doctor approval."
            });
        }

        // ── PUT /api/prescriptions/{id} ──────────────────────────────────────
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePrescriptionDto dto)
        {
            var prescription = await _context.Prescriptions.FindAsync(id);
            if (prescription is null) return NotFound(new { message = "Prescription not found." });

            if (dto.Notes             is not null) prescription.Notes             = dto.Notes;
            if (dto.IssuedByDoctorId  is not null) prescription.IssuedByDoctorId  = dto.IssuedByDoctorId;

            prescription.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(prescription);
        }

        // ── PATCH /api/prescriptions/{id}/approve ────────────────────────────
        /// <summary>
        /// Human-in-the-loop: Doctor approves or rejects the AI-validated prescription.
        /// On Approve → Status = "Issued" and notification is simulated.
        /// On Reject  → Status = "Cancelled".
        /// </summary>
        [HttpPatch("{id:guid}/approve")]
        public async Task<IActionResult> Approve(Guid id, [FromBody] ApprovePrescriptionDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var prescription = await _context.Prescriptions
                .Include(p => p.Patient)
                .Include(p => p.Items)
                    .ThenInclude(i => i.Medicine)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (prescription is null) return NotFound(new { message = "Prescription not found." });

            if (prescription.Status != "Draft")
                return BadRequest(new { message = $"Prescription is already '{prescription.Status}'. Only Draft prescriptions can be approved/rejected." });

            if (dto.Decision.Equals("Approved", StringComparison.OrdinalIgnoreCase))
            {
                prescription.Status            = "Issued";
                prescription.IssuedByDoctorId  = dto.DoctorId;
                prescription.Notes             = dto.DoctorNotes ?? prescription.Notes;

                // ── Trigger third-party SMS & Email notification (Component D requirement) ──
                prescription.NotificationSent    = true;
                prescription.NotificationChannel = "Email & SMS";
                prescription.NotifiedAt          = DateTime.UtcNow;

                var medSummary = string.Join(", ", prescription.Items.Select(i => $"{i.Medicine.Name} x{i.Quantity} ({i.Dosage})"));
                await _notificationService.DispatchPrescriptionNotificationAsync(
                    prescription.Patient?.FullName ?? "Patient",
                    "patient@careflow.hospital.org",
                    medSummary,
                    "Both"
                );
            }
            else if (dto.Decision.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
            {
                prescription.Status = "Cancelled";
                prescription.Notes  = dto.DoctorNotes ?? prescription.Notes;
            }
            else
            {
                return BadRequest(new { message = "Decision must be 'Approved' or 'Rejected'." });
            }

            prescription.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                prescription.Id,
                prescription.Status,
                prescription.NotificationSent,
                prescription.NotificationChannel,
                message = dto.Decision.Equals("Approved", StringComparison.OrdinalIgnoreCase)
                    ? "Prescription issued. Patient notified via Third-Party Email and SMS."
                    : "Prescription rejected and cancelled."
            });
        }

        // ── PATCH /api/prescriptions/{id}/dispense ───────────────────────────
        /// <summary>
        /// Pharmacist dispenses the prescription: deducts stock for each item.
        /// Prescription must be in "Issued" status to dispense.
        /// </summary>
        [HttpPatch("{id:guid}/dispense")]
        public async Task<IActionResult> Dispense(Guid id)
        {
            var prescription = await _context.Prescriptions
                .Include(p => p.Items)
                    .ThenInclude(i => i.Medicine)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (prescription is null)
                return NotFound(new { message = "Prescription not found." });

            if (prescription.Status != "Issued")
                return BadRequest(new { message = $"Only 'Issued' prescriptions can be dispensed. Current status: '{prescription.Status}'." });

            // Check stock before deducting
            foreach (var item in prescription.Items)
            {
                if (item.Medicine.StockQuantity < item.Quantity)
                    return BadRequest(new
                    {
                        message = $"Insufficient stock for '{item.Medicine.Name}'. Available: {item.Medicine.StockQuantity}, Required: {item.Quantity}."
                    });
            }

            // Deduct stock for each line item
            foreach (var item in prescription.Items)
            {
                item.Medicine.StockQuantity -= item.Quantity;
                item.Medicine.UpdatedAt      = DateTime.UtcNow;
            }

            prescription.Status    = "Dispensed";
            prescription.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                prescription.Id,
                prescription.Status,
                message  = "Prescription dispensed. Stock deducted successfully.",
                Dispensed = prescription.Items.Select(i => new
                {
                    Medicine         = i.Medicine.Name,
                    QuantityDispensed = i.Quantity,
                    RemainingStock   = i.Medicine.StockQuantity
                })
            });
        }

        // ── DELETE /api/prescriptions/{id} ──────────────────────────────────
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var prescription = await _context.Prescriptions.FindAsync(id);
            if (prescription is null) return NotFound(new { message = "Prescription not found." });

            if (prescription.Status is "Issued" or "Dispensed")
                return BadRequest(new { message = "Cannot delete an Issued or Dispensed prescription. Cancel it first." });

            _context.Prescriptions.Remove(prescription);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Prescription deleted." });
        }
    }
}
