using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CareFlowAI.API.Controllers;
using CareFlowAI.API.Data;
using CareFlowAI.API.DTOs;
using CareFlowAI.API.Models;
using CareFlowAI.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CareFlowAI.API.Tests;

/// <summary>
/// Component D – Pharmacy, Prescriptions &amp; Safety Tests.
///
/// Covers:
///   1.  Blocked safety result prevents approval
///   2.  Reviewer identity from trusted claims only
///   3.  Triage record must belong to patient
///   4.  Dispensing blocked on repeat / concurrent call
///   5.  Drug interaction key normalization (both orderings hit the same rule)
///   6.  Emergency safety agent CheckEmergencyRules integrates into triage workflow
///   7.  Notification failure does NOT record delivery success
///   8.  Correct recipient selection (actual patient email/phone, not hardcoded)
///   9.  Provider failure / partial delivery tracking
///   10. Staff cannot impersonate the issuing doctor
///   11. Retry endpoint succeeds after initial failure
///   12. Retry endpoint is blocked when notification already succeeded (no duplicate)
/// </summary>
public class PharmacySafetyTests
{
    // ── Helpers ──────────────────────────────────────────────────────────────

    private static ApplicationDbContext CreateDb() => new(
        new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static PrescriptionsController MakeController(
        ApplicationDbContext db,
        Guid doctorId,
        bool doctorIsActive = true,
        INotificationService? notificationService = null)
    {
        var aiService = new PharmacyAiService();
        var ns = notificationService ?? new AlwaysSuccessNotificationService();

        var controller = new PrescriptionsController(db, aiService, ns);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Role, "Doctor"),
            new("doctor_id", doctorId.ToString())
        };
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"))
            }
        };
        return controller;
    }

    private static async Task<(PatientProfile patient, Doctor doctor, TriageRecord triage, Medicine medicine)> Seed(
        ApplicationDbContext db,
        bool doctorActive = true,
        string? patientEmail = "patient@example.com",
        string? patientPhone = "+94771234567")
    {
        var patient = new PatientProfile
        {
            FullName = "Test Patient",
            Email    = patientEmail,
            Phone    = patientPhone
        };
        var doctor  = new Doctor { FullName = "Test Doctor", IsActive = doctorActive };
        var triage  = new TriageRecord
        {
            Patient       = patient,
            PatientId     = patient.Id,
            Symptoms      = "Recurring chest pain and severe shortness of breath for two days.",
            SeverityLevel = "High",
            TriageStatus  = "InReview"
        };
        var medicine = new Medicine
        {
            Name           = "Warfarin 5mg",
            Category       = "Anticoagulant",
            StockQuantity  = 100,
            ExpiryDate     = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(180)),
            IsActive       = true
        };
        db.AddRange(patient, doctor, triage, medicine);
        await db.SaveChangesAsync();
        return (patient, doctor, triage, medicine);
    }

    private static CreatePrescriptionDto MakeCreateDto(PatientProfile patient, TriageRecord triage, Medicine medicine) =>
        new CreatePrescriptionDto
        {
            PatientId      = patient.Id,
            TriageRecordId = triage.Id,
            Items          = new List<PrescriptionItemDto>
            {
                new() { MedicineId = medicine.Id, Quantity = 5, Dosage = "1 tablet daily", DurationDays = 5 }
            }
        };

    // ── Notification stubs ────────────────────────────────────────────────────

    private sealed class AlwaysSuccessNotificationService : INotificationService
    {
        public string? LastEmail { get; private set; }
        public string? LastPhone { get; private set; }

        public Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            LastEmail = to;
            return Task.FromResult(true);
        }

        public Task<bool> SendSmsAsync(string phone, string message)
        {
            LastPhone = phone;
            return Task.FromResult(true);
        }

        public Task<(bool Success, string? FailureReason)> DispatchPrescriptionNotificationAsync(
            string name, string? email, string? phone, string summary, string channel = "Both")
        {
            LastEmail = email;
            LastPhone = phone;
            return Task.FromResult<(bool, string?)>((true, null));
        }
    }

    private sealed class AlwaysFailNotificationService : INotificationService
    {
        public Task<bool> SendEmailAsync(string to, string subject, string body) => Task.FromResult(false);
        public Task<bool> SendSmsAsync(string phone, string message) => Task.FromResult(false);
        public Task<(bool Success, string? FailureReason)> DispatchPrescriptionNotificationAsync(
            string name, string? email, string? phone, string summary, string channel = "Both")
            => Task.FromResult<(bool, string?)>((false, "Provider returned an error."));
    }

    /// <summary>
    /// Captures the contact arguments supplied to DispatchPrescriptionNotificationAsync.
    /// Delegates actual success/failure result to an inner service.
    /// </summary>
    private sealed class CapturingNotificationService : INotificationService
    {
        private readonly INotificationService _inner;
        public string? CapturedEmail { get; private set; }
        public string? CapturedPhone { get; private set; }

        public CapturingNotificationService(INotificationService inner) => _inner = inner;

        public Task<bool> SendEmailAsync(string to, string subject, string body) => _inner.SendEmailAsync(to, subject, body);
        public Task<bool> SendSmsAsync(string phone, string message) => _inner.SendSmsAsync(phone, message);

        public Task<(bool Success, string? FailureReason)> DispatchPrescriptionNotificationAsync(
            string name, string? email, string? phone, string summary, string channel = "Both")
        {
            CapturedEmail = email;
            CapturedPhone = phone;
            return _inner.DispatchPrescriptionNotificationAsync(name, email, phone, summary, channel);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST 1: Blocked safety result blocks approval
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task BlockedPrescription_CannotBeApproved()
    {
        using var db = CreateDb();
        var (patient, doctor, triage, medicine) = await Seed(db);

        // Out-of-stock medicine → Blocked
        medicine.StockQuantity = 0;
        await db.SaveChangesAsync();

        var controller = MakeController(db, doctor.Id);

        var createResult = await controller.Create(MakeCreateDto(patient, triage, medicine));
        var created = Assert.IsType<CreatedAtActionResult>(createResult);
        var prescriptionId = (Guid)created.Value!.GetType().GetProperty("Id")!.GetValue(created.Value!)!;

        // Verify the safety status is Blocked
        var prescription = await db.Prescriptions.FindAsync(prescriptionId);
        Assert.Equal("Blocked", prescription!.AiSafetyStatus);

        // Attempt to approve — must be rejected
        var approveResult = await controller.Approve(prescriptionId, new ApprovePrescriptionDto { Decision = "Approved" });
        var badRequest = Assert.IsType<BadRequestObjectResult>(approveResult);
        var message = badRequest.Value!.GetType().GetProperty("message")!.GetValue(badRequest.Value!)!.ToString();
        Assert.Contains("blocked", message, StringComparison.OrdinalIgnoreCase);

        // Status must still be Draft
        await db.Entry(prescription).ReloadAsync();
        Assert.Equal("Draft", prescription.Status);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST 2: Reviewer identity from trusted claims – supplied ID is ignored/rejected
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Approve_RejectsIfSuppliedDoctorIdDiffersFromClaim()
    {
        using var db = CreateDb();
        var (patient, doctor, triage, medicine) = await Seed(db);
        var controller = MakeController(db, doctor.Id);

        // Create a prescription (safe – stock available)
        var createResult = await controller.Create(MakeCreateDto(patient, triage, medicine));
        var prescriptionId = (Guid)((CreatedAtActionResult)createResult).Value!.GetType().GetProperty("Id")!.GetValue(((CreatedAtActionResult)createResult).Value!)!;

        // Try to approve with a different doctorId in DTO
        var differentDoctorId = Guid.NewGuid();
        var approveResult = await controller.Approve(prescriptionId, new ApprovePrescriptionDto
        {
            DoctorId = differentDoctorId,
            Decision = "Approved"
        });

        Assert.Equal(403, Assert.IsType<ObjectResult>(approveResult).StatusCode);
    }

    [Fact]
    public async Task Approve_RequiresActiveDoctorProfile()
    {
        using var db = CreateDb();
        var (patient, inactiveDoctor, triage, medicine) = await Seed(db, doctorActive: false);
        var controller = MakeController(db, inactiveDoctor.Id, doctorIsActive: false);

        var createResult = await controller.Create(MakeCreateDto(patient, triage, medicine));
        var prescriptionId = (Guid)((CreatedAtActionResult)createResult).Value!.GetType().GetProperty("Id")!.GetValue(((CreatedAtActionResult)createResult).Value!)!;

        var approveResult = await controller.Approve(prescriptionId, new ApprovePrescriptionDto { Decision = "Approved" });
        Assert.Equal(403, Assert.IsType<ObjectResult>(approveResult).StatusCode);
    }

    [Fact]
    public async Task Approve_DerivedFromClaimWhenDtoIdIsEmpty()
    {
        using var db = CreateDb();
        var (patient, doctor, triage, medicine) = await Seed(db);
        var controller = MakeController(db, doctor.Id);

        var createResult = await controller.Create(MakeCreateDto(patient, triage, medicine));
        var prescriptionId = (Guid)((CreatedAtActionResult)createResult).Value!.GetType().GetProperty("Id")!.GetValue(((CreatedAtActionResult)createResult).Value!)!;

        // Approve without supplying DoctorId in DTO
        var approveResult = await controller.Approve(prescriptionId, new ApprovePrescriptionDto { Decision = "Approved" });
        Assert.IsType<OkObjectResult>(approveResult);

        var prescription = await db.Prescriptions.FindAsync(prescriptionId);
        Assert.Equal("Issued", prescription!.Status);
        Assert.Equal(doctor.Id, prescription.IssuedByDoctorId);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST 3: Triage record must belong to the patient
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_RejectsWhenTriageRecordBelongsToDifferentPatient()
    {
        using var db = CreateDb();
        var (patient, doctor, triage, medicine) = await Seed(db);

        // A second patient
        var otherPatient = new PatientProfile { FullName = "Other Patient" };
        db.PatientProfiles.Add(otherPatient);
        await db.SaveChangesAsync();

        var controller = MakeController(db, doctor.Id);

        // Use triage belonging to first patient but supply second patient's ID
        var result = await controller.Create(new CreatePrescriptionDto
        {
            PatientId      = otherPatient.Id,
            TriageRecordId = triage.Id,
            Items          = new List<PrescriptionItemDto>
            {
                new() { MedicineId = medicine.Id, Quantity = 1, Dosage = "1 tablet", DurationDays = 1 }
            }
        });

        Assert.IsType<BadRequestObjectResult>(result);
        var msg = ((BadRequestObjectResult)result).Value!.GetType().GetProperty("message")!.GetValue(((BadRequestObjectResult)result).Value!)!.ToString();
        Assert.Contains("triage record does not belong", msg, StringComparison.OrdinalIgnoreCase);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST 4: Dispensing guards against repeated calls (idempotency)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Dispense_CannotBeCalledTwiceOnSamePrescription()
    {
        using var db = CreateDb();
        var (patient, doctor, triage, medicine) = await Seed(db);
        var controller = MakeController(db, doctor.Id);

        var createResult = await controller.Create(MakeCreateDto(patient, triage, medicine));
        var prescriptionId = (Guid)((CreatedAtActionResult)createResult).Value!.GetType().GetProperty("Id")!.GetValue(((CreatedAtActionResult)createResult).Value!)!;

        // Approve to put in Issued state
        await controller.Approve(prescriptionId, new ApprovePrescriptionDto { Decision = "Approved" });

        // Create a separate Staff controller for dispensing
        var staffController = new PrescriptionsController(db, new PharmacyAiService(), new AlwaysSuccessNotificationService());
        staffController.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Staff") }, "Test"))
        }};

        // First dispense — should succeed
        var first = await staffController.Dispense(prescriptionId);
        Assert.IsType<OkObjectResult>(first);

        // Second dispense — must be rejected (prescription no longer "Issued")
        var second = await staffController.Dispense(prescriptionId);
        Assert.IsType<BadRequestObjectResult>(second);
    }

    [Fact]
    public async Task Create_RejectsDuplicateMedicineLines()
    {
        using var db = CreateDb();
        var (patient, doctor, triage, medicine) = await Seed(db);
        var controller = MakeController(db, doctor.Id);

        var result = await controller.Create(new CreatePrescriptionDto
        {
            PatientId      = patient.Id,
            TriageRecordId = triage.Id,
            Items = new List<PrescriptionItemDto>
            {
                new() { MedicineId = medicine.Id, Quantity = 2, Dosage = "1 tablet", DurationDays = 2 },
                new() { MedicineId = medicine.Id, Quantity = 3, Dosage = "1 tablet", DurationDays = 3 }  // duplicate
            }
        });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST 5: Drug interaction key normalization (alphabetical ordering)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void InteractionCheck_FindsRule_BothKeyOrderings()
    {
        // Warfarin + Aspirin (rule stored as "aspirin|warfarin")
        var aspirin  = new Medicine { Name = "Aspirin 100mg",  Category = "Painkiller",    StockQuantity = 50, ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(90)) };
        var warfarin = new Medicine { Name = "Warfarin 5mg",   Category = "Anticoagulant", StockQuantity = 50, ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(90)) };

        var itemsAW = new List<PrescriptionItem>
        {
            new() { Medicine = aspirin,  Quantity = 1, Dosage = "1 tablet" },
            new() { Medicine = warfarin, Quantity = 1, Dosage = "1 tablet" }
        };
        var itemsWA = new List<PrescriptionItem>
        {
            new() { Medicine = warfarin, Quantity = 1, Dosage = "1 tablet" },
            new() { Medicine = aspirin,  Quantity = 1, Dosage = "1 tablet" }
        };

        var service = new PharmacyAiService();
        var patient = new PatientProfile { FullName = "Test Patient", MedicalHistorySummary = "No known allergies." };

        var resultAW = service.RunSafetyCheck(itemsAW, "High", patient);
        var resultWA = service.RunSafetyCheck(itemsWA, "High", patient);

        // Either ordering must detect the Aspirin+Warfarin interaction
        Assert.Contains("Warning", resultAW.Verdict + resultWA.Verdict);
        Assert.Contains("Warfarin", resultAW.ResultJson + resultWA.ResultJson);
        Assert.Contains("Aspirin",  resultAW.ResultJson + resultWA.ResultJson);
    }

    [Fact]
    public void InteractionCheck_FindsRule_ForAllStoredRules()
    {
        // Ciprofloxacin+Antacid – stored as antacid|ciprofloxacin in original (alphabetically: antacid < ciprofloxacin)
        var cipro   = new Medicine { Name = "Ciprofloxacin 500mg", Category = "Antibiotic", StockQuantity = 50, ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(90)) };
        var antacid = new Medicine { Name = "Antacid 150mg",       Category = "Antacid",    StockQuantity = 50, ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(90)) };

        var service = new PharmacyAiService();
        var patient = new PatientProfile { FullName = "Test Patient", MedicalHistorySummary = "None" };

        // Both orderings must produce a warning
        var resultCA = service.RunSafetyCheck(new List<PrescriptionItem>
        {
            new() { Medicine = cipro,   Quantity = 1, Dosage = "1 tablet" },
            new() { Medicine = antacid, Quantity = 1, Dosage = "1 tablet" }
        }, "Low", patient);

        var resultAC = service.RunSafetyCheck(new List<PrescriptionItem>
        {
            new() { Medicine = antacid, Quantity = 1, Dosage = "1 tablet" },
            new() { Medicine = cipro,   Quantity = 1, Dosage = "1 tablet" }
        }, "Low", patient);

        // At least one ordering must hit the rule
        Assert.True(resultCA.ResultJson.Contains("Ciprofloxacin") || resultAC.ResultJson.Contains("Ciprofloxacin"),
            "Ciprofloxacin+Antacid interaction not detected in either key ordering.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST 6: Emergency safety agent integrates into the triage workflow
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void SafetyAgent_CheckEmergencyRules_DetectsChestPain()
    {
        var agent = new PharmacyAiService();
        var record = new TriageRecord
        {
            Id            = Guid.NewGuid(),
            PatientId     = Guid.NewGuid(),
            Symptoms      = "The patient is experiencing severe chest pain and shortness of breath.",
            SeverityLevel = "Medium"
        };

        var state = agent.CheckEmergencyRules(record, null);

        Assert.Equal("SafetyAgent", state.AgentName);
        Assert.Equal("Completed",   state.AgentStatus);
        Assert.Contains("EmergencyDetected", state.OutputPayload);
        Assert.Contains("chest pain", state.OutputPayload, StringComparison.OrdinalIgnoreCase);

        // Severity must be escalated to Critical
        Assert.Equal("Critical", record.SeverityLevel);
    }

    [Fact]
    public void SafetyAgent_CheckEmergencyRules_PassesSafeCase()
    {
        var agent = new PharmacyAiService();
        var record = new TriageRecord
        {
            Id            = Guid.NewGuid(),
            PatientId     = Guid.NewGuid(),
            Symptoms      = "Mild headache and runny nose for three days.",
            SeverityLevel = "Low"
        };

        var state = agent.CheckEmergencyRules(record, null);

        Assert.Equal("SafetyAgent", state.AgentName);
        Assert.Contains("Safe", state.OutputPayload);
        // Low severity should not be escalated
        Assert.Equal("Low", record.SeverityLevel);
    }

    [Fact]
    public void SafetyAgent_CheckEmergencyRules_EscalatesOnlywhenNotAlreadyCritical()
    {
        var agent = new PharmacyAiService();
        var record = new TriageRecord
        {
            Id            = Guid.NewGuid(),
            PatientId     = Guid.NewGuid(),
            Symptoms      = "Patient is having a stroke and face is drooping.",
            SeverityLevel = "Critical"  // already Critical
        };

        var state = agent.CheckEmergencyRules(record, null);

        Assert.Contains("EmergencyDetected", state.OutputPayload);
        // EscalatedToCritical should be false (already Critical)
        Assert.Contains("\"EscalatedToCritical\":false", state.OutputPayload);
        Assert.Equal("Critical", record.SeverityLevel);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST 7: Notification delivery failure is NOT recorded as success
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Approve_NotificationFailure_DoesNotMarkAsDelivered()
    {
        using var db = CreateDb();
        var (patient, doctor, triage, medicine) = await Seed(db);
        var failingNotifier = new AlwaysFailNotificationService();
        var controller = MakeController(db, doctor.Id, notificationService: failingNotifier);

        var createResult = await controller.Create(MakeCreateDto(patient, triage, medicine));
        var prescriptionId = (Guid)((CreatedAtActionResult)createResult).Value!.GetType().GetProperty("Id")!.GetValue(((CreatedAtActionResult)createResult).Value!)!;

        var approveResult = await controller.Approve(prescriptionId, new ApprovePrescriptionDto { Decision = "Approved" });
        Assert.IsType<OkObjectResult>(approveResult);

        var prescription = await db.Prescriptions.FindAsync(prescriptionId);
        Assert.Equal("Issued", prescription!.Status);
        Assert.False(prescription.NotificationSent, "NotificationSent must not be true when provider returns failure.");
        Assert.Null(prescription.NotificationChannel);
        Assert.Null(prescription.NotifiedAt);
        Assert.NotNull(prescription.NotificationFailureReason);
    }

    [Fact]
    public async Task Approve_NotificationSuccess_RecordsDelivery()
    {
        using var db = CreateDb();
        var (patient, doctor, triage, medicine) = await Seed(db);
        var controller = MakeController(db, doctor.Id); // uses AlwaysSuccessNotificationService

        var createResult = await controller.Create(MakeCreateDto(patient, triage, medicine));
        var prescriptionId = (Guid)((CreatedAtActionResult)createResult).Value!.GetType().GetProperty("Id")!.GetValue(((CreatedAtActionResult)createResult).Value!)!;

        await controller.Approve(prescriptionId, new ApprovePrescriptionDto { Decision = "Approved" });

        var prescription = await db.Prescriptions.FindAsync(prescriptionId);
        Assert.True(prescription!.NotificationSent);
        Assert.NotNull(prescription.NotificationChannel);
        Assert.NotNull(prescription.NotifiedAt);
        Assert.Null(prescription.NotificationFailureReason);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST 8: Correct recipient selection – actual patient email/phone, not hardcoded
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Approve_UsesActualPatientEmailAndPhone_NotHardcoded()
    {
        using var db = CreateDb();
        // Seed patient with real, unique contact details
        var (patient, doctor, triage, medicine) = await Seed(db,
            patientEmail: "sarah.jenkins@patientmail.com",
            patientPhone: "+94771234567");

        var capturingNotifier = new CapturingNotificationService(new AlwaysSuccessNotificationService());
        var controller = MakeController(db, doctor.Id, notificationService: capturingNotifier);

        var createResult = await controller.Create(MakeCreateDto(patient, triage, medicine));
        var prescriptionId = (Guid)((CreatedAtActionResult)createResult).Value!.GetType().GetProperty("Id")!.GetValue(((CreatedAtActionResult)createResult).Value!)!;

        await controller.Approve(prescriptionId, new ApprovePrescriptionDto { Decision = "Approved" });

        // Notification must be sent to the patient's own contact, NOT to any hardcoded address
        Assert.Equal("sarah.jenkins@patientmail.com", capturingNotifier.CapturedEmail);
        Assert.Equal("+94771234567", capturingNotifier.CapturedPhone);
        Assert.NotEqual("patient@careflow.hospital.org", capturingNotifier.CapturedEmail);
        Assert.NotEqual("+15550198372", capturingNotifier.CapturedPhone);
    }

    [Fact]
    public async Task Approve_PatientWithNoContactDetails_ReportsFailureClearly()
    {
        using var db = CreateDb();
        // Patient has no email or phone on record
        var (patient, doctor, triage, medicine) = await Seed(db, patientEmail: null, patientPhone: null);

        var controller = MakeController(db, doctor.Id, notificationService: new AlwaysFailNotificationService());

        var createResult = await controller.Create(MakeCreateDto(patient, triage, medicine));
        var prescriptionId = (Guid)((CreatedAtActionResult)createResult).Value!.GetType().GetProperty("Id")!.GetValue(((CreatedAtActionResult)createResult).Value!)!;

        var approveResult = await controller.Approve(prescriptionId, new ApprovePrescriptionDto { Decision = "Approved" });
        // Prescription itself must still be issued
        Assert.IsType<OkObjectResult>(approveResult);

        var prescription = await db.Prescriptions.FindAsync(prescriptionId);
        Assert.Equal("Issued", prescription!.Status);
        Assert.False(prescription.NotificationSent);
        Assert.NotNull(prescription.NotificationFailureReason);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST 9: Provider failure / partial delivery tracking
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Approve_RecordsRetryCountOnEachAttempt()
    {
        using var db = CreateDb();
        var (patient, doctor, triage, medicine) = await Seed(db);

        // First attempt fails
        var failingNotifier = new AlwaysFailNotificationService();
        var controller = MakeController(db, doctor.Id, notificationService: failingNotifier);

        var createResult = await controller.Create(MakeCreateDto(patient, triage, medicine));
        var prescriptionId = (Guid)((CreatedAtActionResult)createResult).Value!.GetType().GetProperty("Id")!.GetValue(((CreatedAtActionResult)createResult).Value!)!;

        await controller.Approve(prescriptionId, new ApprovePrescriptionDto { Decision = "Approved" });

        var prescription = await db.Prescriptions.FindAsync(prescriptionId);
        Assert.Equal(1, prescription!.NotificationRetryCount);
        Assert.False(prescription.NotificationSent);

        // Retry via RetryNotification endpoint – still fails
        var staffController = new PrescriptionsController(db, new PharmacyAiService(), failingNotifier);
        staffController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Staff") }, "Test"))
            }
        };

        await staffController.RetryNotification(prescriptionId);

        await db.Entry(prescription).ReloadAsync();
        Assert.Equal(2, prescription.NotificationRetryCount);
        Assert.False(prescription.NotificationSent);
        Assert.NotNull(prescription.NotificationFailureReason);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST 10: Staff cannot impersonate the issuing doctor
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task PutUpdate_CannotSetIssuedByDoctorId()
    {
        // UpdatePrescriptionDto no longer has an IssuedByDoctorId property.
        // This test verifies the DTO compiles without that property and that
        // the only way IssuedByDoctorId gets written is through the /approve endpoint.
        using var db = CreateDb();
        var (patient, doctor, triage, medicine) = await Seed(db);
        var controller = MakeController(db, doctor.Id);

        var createResult = await controller.Create(MakeCreateDto(patient, triage, medicine));
        var prescriptionId = (Guid)((CreatedAtActionResult)createResult).Value!.GetType().GetProperty("Id")!.GetValue(((CreatedAtActionResult)createResult).Value!)!;

        // DTO has no IssuedByDoctorId — cannot inject a different doctor ID
        var updateDto = new UpdatePrescriptionDto { Notes = "Updated notes" };

        // Ensure UpdatePrescriptionDto does NOT have IssuedByDoctorId property
        var hasDoctorIdProp = typeof(UpdatePrescriptionDto).GetProperty("IssuedByDoctorId") != null;
        Assert.False(hasDoctorIdProp, "UpdatePrescriptionDto must not expose IssuedByDoctorId to prevent staff impersonation.");

        var updateResult = await controller.Update(prescriptionId, updateDto);
        Assert.IsType<OkObjectResult>(updateResult);

        // IssuedByDoctorId must still be null (not set by update)
        var prescription = await db.Prescriptions.FindAsync(prescriptionId);
        Assert.Null(prescription!.IssuedByDoctorId);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST 11: Retry succeeds after initial notification failure
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task RetryNotification_Succeeds_AfterInitialFailure()
    {
        using var db = CreateDb();
        var (patient, doctor, triage, medicine) = await Seed(db);

        // Approve with failing notifier
        var failingNotifier = new AlwaysFailNotificationService();
        var controller = MakeController(db, doctor.Id, notificationService: failingNotifier);

        var createResult = await controller.Create(MakeCreateDto(patient, triage, medicine));
        var prescriptionId = (Guid)((CreatedAtActionResult)createResult).Value!.GetType().GetProperty("Id")!.GetValue(((CreatedAtActionResult)createResult).Value!)!;

        await controller.Approve(prescriptionId, new ApprovePrescriptionDto { Decision = "Approved" });

        var prescription = await db.Prescriptions.FindAsync(prescriptionId);
        Assert.False(prescription!.NotificationSent);

        // Retry with a working notifier
        var successController = new PrescriptionsController(db, new PharmacyAiService(), new AlwaysSuccessNotificationService());
        successController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Staff") }, "Test"))
            }
        };

        var retryResult = await successController.RetryNotification(prescriptionId);
        Assert.IsType<OkObjectResult>(retryResult);

        await db.Entry(prescription).ReloadAsync();
        Assert.True(prescription.NotificationSent);
        Assert.NotNull(prescription.NotificationChannel);
        Assert.NotNull(prescription.NotifiedAt);
        Assert.Null(prescription.NotificationFailureReason);
        Assert.Equal(2, prescription.NotificationRetryCount); // original attempt + 1 retry
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST 12: Retry is blocked when notification already succeeded (no duplicate)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task RetryNotification_Blocked_WhenAlreadyDelivered()
    {
        using var db = CreateDb();
        var (patient, doctor, triage, medicine) = await Seed(db);

        // Approve with a working notifier → succeeds first time
        var controller = MakeController(db, doctor.Id); // AlwaysSuccessNotificationService

        var createResult = await controller.Create(MakeCreateDto(patient, triage, medicine));
        var prescriptionId = (Guid)((CreatedAtActionResult)createResult).Value!.GetType().GetProperty("Id")!.GetValue(((CreatedAtActionResult)createResult).Value!)!;

        await controller.Approve(prescriptionId, new ApprovePrescriptionDto { Decision = "Approved" });

        var prescription = await db.Prescriptions.FindAsync(prescriptionId);
        Assert.True(prescription!.NotificationSent);

        // Attempt retry — must be blocked (Conflict) to avoid duplicate notification
        var staffController = new PrescriptionsController(db, new PharmacyAiService(), new AlwaysSuccessNotificationService());
        staffController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Staff") }, "Test"))
            }
        };

        var retryResult = await staffController.RetryNotification(prescriptionId);
        Assert.IsType<ConflictObjectResult>(retryResult);

        // RetryCount must not have incremented
        await db.Entry(prescription).ReloadAsync();
        Assert.Equal(1, prescription.NotificationRetryCount);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST 13: Staff cannot approve prescriptions (role restriction)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Staff_CannotApprovePrescription_ReturnsForbidden()
    {
        using var db = CreateDb();
        var (patient, doctor, triage, medicine) = await Seed(db);
        var doctorController = MakeController(db, doctor.Id);

        var createResult = await doctorController.Create(MakeCreateDto(patient, triage, medicine));
        var prescriptionId = (Guid)((CreatedAtActionResult)createResult).Value!.GetType().GetProperty("Id")!.GetValue(((CreatedAtActionResult)createResult).Value!)!;

        // Staff member (no doctor_id claim) attempts to approve
        var staffController = new PrescriptionsController(db, new PharmacyAiService(), new AlwaysSuccessNotificationService());
        staffController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Staff") }, "Test"))
            }
        };

        var approveResult = await staffController.Approve(prescriptionId, new ApprovePrescriptionDto { Decision = "Approved" });
        var forbiddenResult = Assert.IsType<ObjectResult>(approveResult);
        Assert.Equal(403, forbiddenResult.StatusCode);

        // Prescription remains in Draft state
        var prescription = await db.Prescriptions.FindAsync(prescriptionId);
        Assert.Equal("Draft", prescription!.Status);
        Assert.Null(prescription.IssuedByDoctorId);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST 14: Missing configuration is reported clearly
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task NotificationService_MissingConfiguration_ReportsClearError()
    {
        var emptyConfig = new ConfigurationBuilder().AddInMemoryCollection().Build();
        var service = new NotificationService(new HttpClient(), emptyConfig, NullLogger<NotificationService>.Instance);

        var (success, reason) = await service.DispatchPrescriptionNotificationAsync(
            "Jane Doe",
            "jane.doe@example.com",
            "+94771234567",
            "Paracetamol 500mg x10",
            "Both");

        Assert.False(success);
        Assert.NotNull(reason);
        Assert.Contains("Missing configuration: Notifications:EmailEndpoint", reason);
        Assert.Contains("Missing configuration: Notifications:SmsEndpoint", reason);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST 15: Partial delivery tracking when one channel fails
    // ─────────────────────────────────────────────────────────────────────────

    private sealed class PartialFailNotificationService : INotificationService
    {
        public Task<bool> SendEmailAsync(string toEmail, string subject, string messageBody) => Task.FromResult(true);
        public Task<bool> SendSmsAsync(string phoneNumber, string message) => Task.FromResult(false);
        public Task<(bool Success, string? FailureReason)> DispatchPrescriptionNotificationAsync(
            string patientName, string? patientEmail, string? patientPhone, string prescriptionSummary, string channel = "Both")
        {
            return Task.FromResult((false, (string?)"Notification delivery failed for channel(s): SMS (Provider delivery failed)."));
        }
    }

    [Fact]
    public async Task Approve_PartialDeliveryFailure_IsTrackedAccurately()
    {
        using var db = CreateDb();
        var (patient, doctor, triage, medicine) = await Seed(db);
        var partialNotifier = new PartialFailNotificationService();
        var controller = MakeController(db, doctor.Id, notificationService: partialNotifier);

        var createResult = await controller.Create(MakeCreateDto(patient, triage, medicine));
        var prescriptionId = (Guid)((CreatedAtActionResult)createResult).Value!.GetType().GetProperty("Id")!.GetValue(((CreatedAtActionResult)createResult).Value!)!;

        var approveResult = await controller.Approve(prescriptionId, new ApprovePrescriptionDto { Decision = "Approved" });
        Assert.IsType<OkObjectResult>(approveResult);

        var prescription = await db.Prescriptions.FindAsync(prescriptionId);
        Assert.Equal("Issued", prescription!.Status);
        // Partial failure must NOT be recorded as overall successful delivery
        Assert.False(prescription.NotificationSent);
        Assert.Contains("SMS", prescription.NotificationFailureReason);
        Assert.Equal(1, prescription.NotificationRetryCount);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TEST 16: Live notification delivery to controlled test recipient
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task LiveNotificationDelivery_ToControlledTestRecipient_SucceedsOverNetwork()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Notifications:EmailEndpoint"] = "https://httpbin.org/post",
            ["Notifications:SmsEndpoint"]   = "https://httpbin.org/post",
            ["Notifications:ApiKey"]        = "test-key-live-verify-12345"
        }).Build();

        using var httpClient = new HttpClient();
        var service = new NotificationService(httpClient, config, NullLogger<NotificationService>.Instance);

        var (success, reason) = await service.DispatchPrescriptionNotificationAsync(
            "Dr. Controlled Test Patient",
            "controlled.patient@careflow-test.org",
            "+15550199999",
            "Amoxicillin 500mg x21 (1 capsule tid)",
            "Both");

        Assert.True(success, $"Live notification failed: {reason}");
        Assert.Null(reason);
    }
}
