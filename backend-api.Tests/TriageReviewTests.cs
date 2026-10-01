using System.Security.Claims;
using System.Text.Json;
using CareFlowAI.API.Controllers;
using CareFlowAI.API.Data;
using CareFlowAI.API.DTOs;
using CareFlowAI.API.Models;
using CareFlowAI.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CareFlowAI.API.Tests;

public class TriageReviewTests
{
    private static string Plan => JsonSerializer.Serialize(new ClinicalPlan
    {
        UrgencyLevel = "Medium", SuggestedSpecialist = "General Practitioner",
        RecommendedAction = "Review the case", Rationale = "Test assessment", AnalysisMethod = "Test"
    });

    private static ApplicationDbContext Context(string? name = null) => new(new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(name ?? Guid.NewGuid().ToString()).Options);

    private static TriageController Controller(ApplicationDbContext db, Guid? doctorId = null, string? role = "Doctor", bool authenticated = true)
    {
        var claims = new List<Claim>();
        if (role != null) claims.Add(new Claim(ClaimTypes.Role, role));
        if (doctorId != null) claims.Add(new Claim("doctor_id", doctorId.ToString()!));
        return new TriageController(
            db,
            new PlanningAgentService(
                new PatientContextTool(db), 
                new GeminiAssessmentClient(new ConfigurationBuilder().Build()), 
                new CareFlowAI.Orchestrator.Agents.DomainAnalysisAgent(new CareFlowAI.API.Services.DomainContextWrapper(new PatientContextTool(db)), "dummy", "gemini-3.8-flash"),
                new ConfigurationBuilder().Build()),
            new Microsoft.Extensions.DependencyInjection.ServiceCollection().BuildServiceProvider(),
            new PharmacyAiService())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext
            { User = new ClaimsPrincipal(new ClaimsIdentity(claims, authenticated ? "Test" : null)) } }
        };
    }

    private static async Task<(TriageRecord record, Doctor doctor)> Seed(ApplicationDbContext db)
    {
        var patient = new PatientProfile { FullName = "Test Patient" };
        var doctor = new Doctor { FullName = "Test Doctor" };
        var record = new TriageRecord { Patient = patient, PatientId = patient.Id, Symptoms = "Example symptoms for the review test.", TriageStatus = "InReview" };
        record.AgentWorkflows.Add(new AgentWorkflowState { AgentName = "PlanningAgent", AgentStatus = "Completed", ApprovalStatus = "Pending", OutputPayload = Plan });
        db.AddRange(patient, doctor, record);
        await db.SaveChangesAsync();
        return (record, doctor);
    }

    [Theory]
    [InlineData(null)] [InlineData("{}")] [InlineData("not json")]
    [InlineData("{\"UrgencyLevel\":\"Unknown\",\"SuggestedSpecialist\":\"X\",\"RecommendedAction\":\"Y\",\"Rationale\":\"Z\"}")]
    public void Invalid_assessments_are_rejected(string? payload) => Assert.Null(TriageReviewRules.ReadPlan(payload));

    [Fact]
    public async Task Anonymous_queue_and_review_are_denied()
    {
        using var db = Context();
        var controller = Controller(db, authenticated: false);
        Assert.IsType<UnauthorizedObjectResult>(await controller.ReviewQueue(new()));
        Assert.IsType<UnauthorizedObjectResult>(await controller.Review(Guid.NewGuid(), new()));
        Assert.IsType<UnauthorizedObjectResult>(await controller.ReviewDetails(Guid.NewGuid()));
    }

    [Fact]
    public async Task Patient_cannot_review_or_list_staff_queue()
    {
        using var db = Context();
        var controller = Controller(db, role: "Patient");
        Assert.Equal(403, Assert.IsType<ObjectResult>(await controller.ReviewQueue(new())).StatusCode);
        Assert.Equal(403, Assert.IsType<ObjectResult>(await controller.Review(Guid.NewGuid(), new())).StatusCode);
    }

    [Theory]
    [InlineData("Approved")] [InlineData("Rejected")] [InlineData("RevisionRequested")]
    public async Task Valid_decision_is_persisted_with_authenticated_doctor(string decision)
    {
        using var db = Context(); var (record, doctor) = await Seed(db);
        var response = await Controller(db, doctor.Id).Review(record.Id, new()
        { Decision = decision, DoctorNotes = " Reason ", ExpectedUpdatedAt = record.UpdatedAt });
        var dto = Assert.IsType<TriageResponseDto>(Assert.IsType<OkObjectResult>(response).Value);
        Assert.Equal(decision, dto.TriageStatus);
        Assert.Equal(decision, record.AgentWorkflows.Single().ApprovalStatus);
        Assert.Equal(doctor.Id, dto.AssignedDoctorId);
        Assert.Equal("Reason", dto.DoctorNotes);
    }

    [Fact]
    public async Task Missing_or_inactive_doctor_profile_cannot_review()
    {
        using var db = Context(); var (record, doctor) = await Seed(db);
        doctor.IsActive = false; await db.SaveChangesAsync();
        Assert.Equal(403, Assert.IsType<ObjectResult>(await Controller(db, doctor.Id).Review(record.Id, new())).StatusCode);
        Assert.Equal(403, Assert.IsType<ObjectResult>(await Controller(db).Review(record.Id, new())).StatusCode);
    }

    [Theory]
    [InlineData("Rejected")] [InlineData("RevisionRequested")]
    public async Task Rejection_and_revision_require_a_reason(string decision)
    {
        using var db = Context(); var (record, doctor) = await Seed(db);
        Assert.IsType<BadRequestObjectResult>(await Controller(db, doctor.Id).Review(record.Id, new()
        { Decision = decision, DoctorNotes = " ", ExpectedUpdatedAt = record.UpdatedAt }));
        Assert.Equal("InReview", record.TriageStatus);
    }

    [Theory]
    [InlineData("Pending")] [InlineData("Approved")] [InlineData("Rejected")] [InlineData("RevisionRequested")]
    public async Task Non_reviewable_states_cannot_be_overwritten(string status)
    {
        using var db = Context(); var (record, doctor) = await Seed(db); record.TriageStatus = status;
        Assert.IsType<ConflictObjectResult>(await Controller(db, doctor.Id).Review(record.Id, new()
        { Decision = "Approved", ExpectedUpdatedAt = record.UpdatedAt }));
    }

    [Fact]
    public async Task Stale_case_version_is_rejected()
    {
        using var db = Context(); var (record, doctor) = await Seed(db);
        Assert.IsType<ConflictObjectResult>(await Controller(db, doctor.Id).Review(record.Id, new()
        { Decision = "Approved", ExpectedUpdatedAt = record.UpdatedAt.AddSeconds(-1) }));
    }

    [Theory]
    [InlineData("Failed", true)] [InlineData("Running", true)] [InlineData("Completed", false)]
    public async Task Failed_or_invalid_assessment_cannot_be_approved(string status, bool valid)
    {
        using var db = Context(); var (record, doctor) = await Seed(db);
        record.AgentWorkflows.Single().AgentStatus = status;
        if (!valid) record.AgentWorkflows.Single().OutputPayload = "{}";
        Assert.IsType<ConflictObjectResult>(await Controller(db, doctor.Id).Review(record.Id, new()
        { Decision = "Approved", ExpectedUpdatedAt = record.UpdatedAt }));
    }

    [Fact]
    public async Task Older_success_does_not_hide_latest_failed_assessment()
    {
        using var db = Context(); var (record, doctor) = await Seed(db);
        db.AgentWorkflows.Add(new AgentWorkflowState { TriageRecordId = record.Id, AgentName = "PlanningAgent", AgentStatus = "Failed", CreatedAt = DateTime.UtcNow.AddMinutes(1) });
        await db.SaveChangesAsync();
        Assert.IsType<ConflictObjectResult>(await Controller(db, doctor.Id).Review(record.Id, new()
        { Decision = "Approved", ExpectedUpdatedAt = record.UpdatedAt }));
    }

    [Fact]
    public async Task Concurrent_updates_trigger_EF_concurrency_check()
    {
        var name = Guid.NewGuid().ToString();
        using var first = Context(name); var (record, _) = await Seed(first);
        using var second = Context(name); var stale = await second.TriageRecords.SingleAsync();
        record.TriageStatus = "Approved"; record.UpdatedAt = record.UpdatedAt.AddSeconds(1); await first.SaveChangesAsync();
        stale.TriageStatus = "Rejected"; stale.UpdatedAt = stale.UpdatedAt.AddSeconds(2);
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => second.SaveChangesAsync());
    }

    [Fact]
    public async Task Queue_filters_and_paginates()
    {
        using var db = Context(); var (record, _) = await Seed(db);
        var other = new TriageRecord { Patient = record.Patient, Symptoms = "Different symptoms", TriageStatus = "Approved" };
        db.Add(other); await db.SaveChangesAsync();
        var response = Assert.IsType<OkObjectResult>(await Controller(db).ReviewQueue(new() { Status = "InReview", Search = "Test Patient", PageSize = 1 }));
        var json = JsonSerializer.SerializeToElement(response.Value, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        Assert.Equal(1, json.GetProperty("total").GetInt32());
        Assert.Equal(record.Id, json.GetProperty("items")[0].GetProperty("id").GetGuid());
    }

    [Theory]
    [InlineData("")] [InlineData("                    ")] [InlineData("too short")]
    public async Task Invalid_symptoms_are_rejected_before_agent_execution(string symptoms)
    {
        using var db = Context();
        Assert.IsType<BadRequestObjectResult>(await Controller(db).Submit(new() { PatientId = Guid.NewGuid(), Symptoms = symptoms }));
        Assert.Empty(db.TriageRecords);
    }

    [Fact]
    public async Task Revision_by_different_patient_is_rejected()
    {
        using var db = Context(); var (record, _) = await Seed(db);
        var controller = Controller(db, role: "Patient");
        var claims = new List<Claim> { new Claim(ClaimTypes.Role, "Patient"), new Claim("patient_id", Guid.NewGuid().ToString()) };
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test")) } };
        
        var dto = new PatientRevisionDto { UpdatedSymptoms = "New symptoms for revision.", ExpectedUpdatedAt = record.UpdatedAt };
        Assert.Equal(403, Assert.IsType<ObjectResult>(await controller.Revise(record.Id, dto)).StatusCode);
    }

    [Fact]
    public async Task Stale_revision_is_rejected()
    {
        using var db = Context(); var (record, _) = await Seed(db);
        record.TriageStatus = "RevisionRequested";
        await db.SaveChangesAsync();
        
        var controller = Controller(db, role: "Patient");
        var claims = new List<Claim> { new Claim(ClaimTypes.Role, "Patient"), new Claim("patient_id", record.PatientId.ToString()) };
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test")) } };
        
        var dto = new PatientRevisionDto { UpdatedSymptoms = "New symptoms for revision.", ExpectedUpdatedAt = record.UpdatedAt.AddSeconds(-1) };
        Assert.IsType<ConflictObjectResult>(await controller.Revise(record.Id, dto));
    }
}
