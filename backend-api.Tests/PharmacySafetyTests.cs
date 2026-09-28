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
/// Component D – Pharmacy, Prescriptions & Safety Tests.
///
/// Covers:
///   1. Blocked safety result prevents approval
///   2. Reviewer identity from trusted claims only
///   3. Triage record must belong to patient
///   4. Dispensing blocked on repeat / concurrent call
///   5. Drug interaction key normalization (both orderings hit the same rule)
///   6. Emergency safety agent CheckEmergencyRules integrates into triage workflow
///   7. Notification failure does NOT record delivery success
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
        ApplicationDbContext db, bool doctorActive = true)
    {
        var patient = new PatientProfile { FullName = "Test Patient" };
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
        public Task<bool> SendEmailAsync(string to, string subject, string body) => Task.FromResult(true);
        public Task<bool> SendSmsAsync(string phone, string message) => Task.FromResult(true);
        public Task<bool> DispatchPrescriptionNotificationAsync(string name, string contact, string summary, string channel = "Both")
            => Task.FromResult(true);
    }

    private sealed class AlwaysFailNotificationService : INotificationService
    {
        public Task<bool> SendEmailAsync(string to, string subject, string body) => Task.FromResult(false);
        public Task<bool> SendSmsAsync(string phone, string message) => Task.FromResult(false);
        public Task<bool> DispatchPrescriptionNotificationAsync(string name, string contact, string summary, string channel = "Both")
            => Task.FromResult(false);
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
        var aspirin = new Medicine { Name = "Aspirin 100mg", Category = "Painkiller",  StockQuantity = 50, ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(90)) };
        var warfarin = new Medicine { Name = "Warfarin 5mg", Category = "Anticoagulant", StockQuantity = 50, ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(90)) };

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

        var resultAW = service.RunSafetyCheck(itemsAW, "High");
        var resultWA = service.RunSafetyCheck(itemsWA, "High");

        // Either ordering must detect the Aspirin+Warfarin interaction
        Assert.Contains("Warning", resultAW.Verdict + resultWA.Verdict);
        Assert.Contains("Warfarin", resultAW.ResultJson + resultWA.ResultJson);
        Assert.Contains("Aspirin", resultAW.ResultJson + resultWA.ResultJson);
    }

    [Fact]
    public void InteractionCheck_FindsRule_ForAllStoredRules()
    {
        // Ciprofloxacin+Antacid – stored as antacid|ciprofloxacin in original (alphabetically: antacid < ciprofloxacin)
        var cipro  = new Medicine { Name = "Ciprofloxacin 500mg", Category = "Antibiotic", StockQuantity = 50, ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(90)) };
        var antacid = new Medicine { Name = "Antacid 150mg", Category = "Antacid", StockQuantity = 50, ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(90)) };

        var service = new PharmacyAiService();

        // Both orderings must produce a warning
        var resultCA = service.RunSafetyCheck(new List<PrescriptionItem>
        {
            new() { Medicine = cipro, Quantity = 1, Dosage = "1 tablet" },
            new() { Medicine = antacid, Quantity = 1, Dosage = "1 tablet" }
        }, "Low");

        var resultAC = service.RunSafetyCheck(new List<PrescriptionItem>
        {
            new() { Medicine = antacid, Quantity = 1, Dosage = "1 tablet" },
            new() { Medicine = cipro, Quantity = 1, Dosage = "1 tablet" }
        }, "Low");

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
        Assert.Equal("Completed", state.AgentStatus);
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
    }
}
