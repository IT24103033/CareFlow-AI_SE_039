using System;
using System.Linq;
using System.Threading.Tasks;
using CareFlowAI.API.Data;
using CareFlowAI.API.DTOs;
using CareFlowAI.API.Models;
using CareFlowAI.API.Services;
using Microsoft.AspNetCore.Authorization;
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
        /// </summary>
        [Authorize(Roles = "Patient,Doctor,Staff,Admin")]
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

            // ── Patient Scoping / Ownership ───────────────────────────────────
            if (User?.IsInRole("Patient") == true)
            {
                var patientClaim = User?.FindFirst("patient_id")?.Value;
                if (!Guid.TryParse(patientClaim, out var authPatientId))
                    return StatusCode(403, "Authenticated patient identity is missing or invalid.");

                if (patientId.HasValue && patientId.Value != authPatientId)
                    return StatusCode(403, "You cannot view prescriptions belonging to another patient.");

                query = query.Where(p => p.PatientId == authPatientId);
            }
            else if (patientId.HasValue)
            {
                query = query.Where(p => p.PatientId == patientId.Value);
            }

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
                    p.NotificationFailureReason,
                    p.NotificationRetryCount,
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
        [Authorize(Roles = "Patient,Doctor,Staff,Admin")]
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

            if (User?.IsInRole("Patient") == true)
            {
                var patientClaim = User?.FindFirst("patient_id")?.Value;
                if (!Guid.TryParse(patientClaim, out var authPatientId) || prescription.PatientId != authPatientId)
                    return StatusCode(403, "You do not have permission to view this prescription.");
            }

            return Ok(prescription);
        }

        private static readonly System.Collections.Concurrent.ConcurrentDictionary<Guid, System.Threading.SemaphoreSlim> _dispenseLocks = new();

        // ── POST /api/prescriptions ──────────────────────────────────────────
        /// <summary>
        /// Creates a Draft prescription, then immediately runs the AI Validation/Safety Agent.
        /// The prescription is paused (stays "Draft") until a doctor approves via PATCH /approve.
        /// </summary>
        [Authorize(Roles = "Doctor,Staff,Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePrescriptionDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Reject duplicate medicine lines
            if (dto.Items.Select(i => i.MedicineId).Distinct().Count() != dto.Items.Count)
                return BadRequest(new { message = "Prescription cannot contain duplicate medicine lines." });

            // Verify patient exists
            var patient = await _context.PatientProfiles.FindAsync(dto.PatientId);
            if (patient == null)
                return NotFound(new { message = $"Patient {dto.PatientId} not found." });

            // Verify triage record exists and get severity
            var triage = await _context.TriageRecords.FindAsync(dto.TriageRecordId);
            if (triage is null)
                return NotFound(new { message = $"Triage record {dto.TriageRecordId} not found." });

            // Ensure a prescription's triage record belongs to its patient
            if (triage.PatientId != dto.PatientId)
                return BadRequest(new { message = "The specified triage record does not belong to the selected patient." });

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
            var safetyResult = _aiService.RunSafetyCheck(prescriptionItems, triage.SeverityLevel, patient);

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
        /// <summary>
        /// Updates mutable prescription metadata (notes only).
        /// IssuedByDoctorId is NOT accepted here; it is derived exclusively from the
        /// authenticated doctor's claim during the /approve action.
        /// </summary>
        [Authorize(Roles = "Doctor,Staff,Admin")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePrescriptionDto dto)
        {
            var prescription = await _context.Prescriptions.FindAsync(id);
            if (prescription is null) return NotFound(new { message = "Prescription not found." });

            if (dto.Notes is not null) prescription.Notes = dto.Notes;

            prescription.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(prescription);
        }

        // ── PATCH /api/prescriptions/{id}/approve ────────────────────────────
        /// <summary>
        /// Human-in-the-loop: Doctor approves or rejects the AI-validated prescription.
        /// On Approve → Status = "Issued" and real provider notification is dispatched to the actual patient.
        /// On Reject  → Status = "Cancelled".
        /// IssuedByDoctorId is derived exclusively from the authenticated doctor_id JWT claim.
        /// </summary>
        [Authorize(Roles = "Doctor")]
        [HttpPatch("{id:guid}/approve")]
        public async Task<IActionResult> Approve(Guid id, [FromBody] ApprovePrescriptionDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Derive the approving doctor exclusively from trusted claims and verify an active doctor
            var doctorClaim = User?.FindFirst("doctor_id")?.Value;
            if (!Guid.TryParse(doctorClaim, out var doctorId))
            {
                return StatusCode(403, new { message = "Authenticated doctor identity is missing or invalid." });
            }

            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Id == doctorId && d.IsActive);
            if (doctor == null)
            {
                return StatusCode(403, new { message = "An active doctor profile is required to approve or reject prescriptions." });
            }

            // Do not accept a different supplied doctor ID
            if (dto.DoctorId.HasValue && dto.DoctorId.Value != Guid.Empty && dto.DoctorId.Value != doctorId)
            {
                return StatusCode(403, new { message = "You cannot approve or reject a prescription on behalf of another doctor." });
            }

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
                // Enforce safety results: blocked prescriptions cannot be approved
                if (string.Equals(prescription.AiSafetyStatus, "Blocked", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new { message = "Cannot approve a prescription that has been blocked by safety checks. Critical errors must be resolved." });
                }

                prescription.Status           = "Issued";
                prescription.IssuedByDoctorId = doctorId;
                prescription.Notes            = dto.DoctorNotes ?? prescription.Notes;

                // ── Dispatch notification to the ACTUAL patient ───────────────
                var medSummary = string.Join(", ", prescription.Items.Select(i => $"{i.Medicine?.Name ?? "Medicine"} x{i.Quantity} ({i.Dosage})"));

                prescription.NotificationRetryCount += 1;

                var (notificationSuccess, failureReason) = await _notificationService.DispatchPrescriptionNotificationAsync(
                    prescription.Patient?.FullName ?? "Patient",
                    prescription.Patient?.Email,
                    prescription.Patient?.Phone,
                    medSummary,
                    "Both"
                );

                // Record delivery outcome accurately
                if (notificationSuccess)
                {
                    prescription.NotificationSent         = true;
                    prescription.NotificationChannel      = "Email & SMS";
                    prescription.NotifiedAt               = DateTime.UtcNow;
                    prescription.NotificationFailureReason = null;
                }
                else
                {
                    prescription.NotificationSent         = false;
                    prescription.NotificationChannel      = null;
                    prescription.NotifiedAt               = null;
                    prescription.NotificationFailureReason = failureReason ?? "Unknown provider error.";
                }
            }
            else if (dto.Decision.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
            {
                prescription.Status           = "Cancelled";
                prescription.IssuedByDoctorId = doctorId;
                prescription.Notes            = dto.DoctorNotes ?? prescription.Notes;
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
                prescription.NotificationFailureReason,
                prescription.NotificationRetryCount,
                message = dto.Decision.Equals("Approved", StringComparison.OrdinalIgnoreCase)
                    ? (prescription.NotificationSent
                        ? "Prescription issued. Patient notified via Third-Party Email and SMS."
                        : $"Prescription issued. Notification delivery to patient failed: {prescription.NotificationFailureReason}")
                    : "Prescription rejected and cancelled."
            });
        }

        // ── PATCH /api/prescriptions/{id}/notify-retry ───────────────────────
        /// <summary>
        /// Retries dispatching the prescription-issued notification to the patient.
        /// Guard: only works on Issued prescriptions where the previous notification failed.
        /// Prevents duplicate notification if already delivered successfully.
        /// </summary>
        [Authorize(Roles = "Doctor,Staff,Admin")]
        [HttpPatch("{id:guid}/notify-retry")]
        public async Task<IActionResult> RetryNotification(Guid id)
        {
            var prescription = await _context.Prescriptions
                .Include(p => p.Patient)
                .Include(p => p.Items)
                    .ThenInclude(i => i.Medicine)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (prescription is null)
                return NotFound(new { message = "Prescription not found." });

            if (prescription.Status != "Issued")
                return BadRequest(new { message = $"Notification retry is only available for Issued prescriptions. Current status: '{prescription.Status}'." });

            if (prescription.NotificationSent)
                return Conflict(new { message = "Notification has already been successfully delivered. Retry is not needed." });

            // Perform retry
            var medSummary = string.Join(", ", prescription.Items.Select(i => $"{i.Medicine?.Name ?? "Medicine"} x{i.Quantity} ({i.Dosage})"));

            prescription.NotificationRetryCount += 1;

            var (notificationSuccess, failureReason) = await _notificationService.DispatchPrescriptionNotificationAsync(
                prescription.Patient?.FullName ?? "Patient",
                prescription.Patient?.Email,
                prescription.Patient?.Phone,
                medSummary,
                "Both"
            );

            if (notificationSuccess)
            {
                prescription.NotificationSent         = true;
                prescription.NotificationChannel      = "Email & SMS";
                prescription.NotifiedAt               = DateTime.UtcNow;
                prescription.NotificationFailureReason = null;
            }
            else
            {
                prescription.NotificationSent         = false;
                prescription.NotificationChannel      = null;
                prescription.NotifiedAt               = null;
                prescription.NotificationFailureReason = failureReason ?? "Unknown provider error.";
            }

            prescription.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                prescription.Id,
                prescription.Status,
                prescription.NotificationSent,
                prescription.NotificationFailureReason,
                prescription.NotificationRetryCount,
                message = notificationSuccess
                    ? "Notification retry succeeded. Patient has been notified."
                    : $"Notification retry failed: {prescription.NotificationFailureReason}"
            });
        }

        // ── PATCH /api/prescriptions/{id}/dispense ───────────────────────────
        /// <summary>
        /// Pharmacist dispenses the prescription: deducts stock for each item.
        /// Protects against concurrent/repeated stock deductions and duplicate medicine lines.
        /// </summary>
        [Authorize(Roles = "Staff,Admin")]
        [HttpPatch("{id:guid}/dispense")]
        public async Task<IActionResult> Dispense(Guid id)
        {
            var myLock = _dispenseLocks.GetOrAdd(id, _ => new System.Threading.SemaphoreSlim(1, 1));
            await myLock.WaitAsync();
            try
            {
                var prescription = await _context.Prescriptions
                    .Include(p => p.Items)
                        .ThenInclude(i => i.Medicine)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (prescription is null)
                    return NotFound(new { message = "Prescription not found." });

                if (prescription.Status != "Issued")
                    return BadRequest(new { message = $"Only 'Issued' prescriptions can be dispensed. Current status: '{prescription.Status}'." });

                // Aggregate quantities by MedicineId to protect against duplicate lines
                var requiredByMedicine = prescription.Items
                    .GroupBy(i => i.MedicineId)
                    .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));

                var medicineIds = requiredByMedicine.Keys.ToList();
                var medicines = await _context.Medicines
                    .Where(m => medicineIds.Contains(m.Id))
                    .ToListAsync();

                // Check stock before deducting
                foreach (var (medId, totalQty) in requiredByMedicine)
                {
                    var medicine = medicines.FirstOrDefault(m => m.Id == medId);
                    if (medicine == null || medicine.StockQuantity < totalQty)
                    {
                        var medName   = medicine?.Name ?? medId.ToString();
                        var available = medicine?.StockQuantity ?? 0;
                        return BadRequest(new
                        {
                            message = $"Insufficient stock for '{medName}'. Available: {available}, Required: {totalQty}."
                        });
                    }
                }

                // Deduct stock for each medicine
                foreach (var (medId, totalQty) in requiredByMedicine)
                {
                    var medicine = medicines.First(m => m.Id == medId);
                    medicine.StockQuantity -= totalQty;
                    medicine.UpdatedAt      = DateTime.UtcNow;
                }

                prescription.Status    = "Dispensed";
                prescription.UpdatedAt = DateTime.UtcNow;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    return Conflict(new { message = "Concurrent modification detected during dispensing. Stock deduction was not repeated." });
                }

                return Ok(new
                {
                    prescription.Id,
                    prescription.Status,
                    message  = "Prescription dispensed. Stock deducted successfully.",
                    Dispensed = requiredByMedicine.Select(kvp => new
                    {
                        Medicine          = medicines.First(m => m.Id == kvp.Key).Name,
                        QuantityDispensed = kvp.Value,
                        RemainingStock    = medicines.First(m => m.Id == kvp.Key).StockQuantity
                    })
                });
            }
            finally
            {
                myLock.Release();
            }
        }

        // ── DELETE /api/prescriptions/{id} ──────────────────────────────────
        [Authorize(Roles = "Doctor,Staff,Admin")]
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
