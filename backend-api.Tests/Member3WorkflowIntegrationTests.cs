using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CareFlowAI.API.Data;
using CareFlowAI.API.DTOs;
using CareFlowAI.API.Models;
using CareFlowAI.API.Services;
using CareFlowAI.Orchestrator.Agents;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace CareFlowAI.API.Tests;

// HTTP integration: real routing/auth/controllers/services, EF InMemory, controlled AI outputs.
// This is not a live Gemini, PostgreSQL or Flutter/React UI test.
public class Member3WorkflowIntegrationTests
{
    private sealed class DomainStub : IDomainAnalysisAgent
    {
        public Task<AgentOutput> AnalyzeRiskAsync(AgentInput input, CancellationToken cancellationToken = default) =>
            Task.FromResult(new AgentOutput { RiskLevel = "Low", RecommendedWardType = "General", PatientHistoryUsed = "Synthetic history" });
    }
    private sealed class Factory : CustomWebApplicationFactory
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IDomainAnalysisAgent>();
                services.AddScoped<IDomainAnalysisAgent, DomainStub>();
                // Prevent background workflow processing from racing the explicit HTTP test.
                services.RemoveAll<IHostedService>();
            });
        }
    }

    [Fact]
    public async Task Patient_submission_booking_doctor_review_and_patient_read_complete_one_workflow()
    {
        await using var factory = new Factory();
        using var patient = factory.CreateClient();
        using var doctorClient = factory.CreateClient();
        var profile = new PatientProfile { FullName = "Member3 Synthetic Patient", MedicalHistorySummary = "Synthetic history", DateOfBirth = new(1990, 1, 1) };
        var doctor = new Doctor { FullName = "Member3 Synthetic Doctor", Email = "member3@example.invalid", Specialization = "General Practitioner", IsActive = true };
        var user = new User { Username = "member3-patient", Role = "Patient", PatientProfileId = profile.Id };
        var doctorUser = new User { Username = "member3-doctor", Role = "Doctor", DoctorId = doctor.Id };
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Database.EnsureCreatedAsync();
            // Keep this isolated database's scenario independent of application seed doctors.
            foreach (var seededDoctor in db.Doctors) seededDoctor.IsActive = false;
            db.PatientProfiles.Add(profile);
            db.Doctors.Add(doctor);
            db.Users.AddRange(user, doctorUser);
            db.DoctorAvailabilities.Add(new DoctorAvailability { DoctorId = doctor.Id, Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), StartTime = new(9, 0), EndTime = new(10, 0) });
            await db.SaveChangesAsync();
        }
        patient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", factory.CreateToken(user.Id, user.Username, "Patient", patientId: profile.Id));
        doctorClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", factory.CreateToken(doctorUser.Id, doctorUser.Username, "Doctor", doctorId: doctor.Id));

        var submitted = await patient.PostAsJsonAsync("/api/triage", new { symptoms = "Persistent discomfort reported for several days.", duration = "three days" });
        Assert.Equal(HttpStatusCode.Created, submitted.StatusCode);
        var triage = (await submitted.Content.ReadFromJsonAsync<TriageResponseDto>())!;
        Assert.Equal(profile.Id, triage.PatientId);
        Assert.Equal("Completed", triage.AiAgentStatus);
        Assert.Equal("Pending", triage.ApprovalStatus);
        Assert.Equal("Safe", triage.SafetyVerdict);
        Assert.Equal("ActionRequired", triage.SchedulingOutcome);
        Assert.NotEmpty(triage.AvailableSlots!);
        var slot = triage.AvailableSlots![0];

        var booked = await patient.PostAsJsonAsync($"/api/triage/{triage.Id}/book-slot", new { appointmentDate = slot.Date, startTime = slot.StartTime, endTime = slot.EndTime });
        Assert.Equal(HttpStatusCode.OK, booked.StatusCode);
        var bookedCase = (await booked.Content.ReadFromJsonAsync<TriageResponseDto>())!;
        Assert.NotNull(bookedCase.TentativeAppointmentId);
        var appointmentId = bookedCase.TentativeAppointmentId.Value;
        var before = await patient.GetFromJsonAsync<AppointmentDto>($"/api/Appointments/{appointmentId}");
        Assert.Equal("Tentative", before!.Status);
        Assert.Equal("Pending", before.ApprovalStatus);
        // Approval enforcement is checked while a real tentative booking exists.
        var premature = await doctorClient.PostAsync($"/api/Appointments/{appointmentId}/confirm", null);
        Assert.Equal(HttpStatusCode.Conflict, premature.StatusCode);
        Assert.Contains("approved", (await premature.Content.ReadAsStringAsync()).ToLowerInvariant());

        var forReview = await doctorClient.GetFromJsonAsync<TriageResponseDto>($"/api/triage/review-queue/{triage.Id}");
        var reviewed = await doctorClient.PatchAsJsonAsync($"/api/triage/{triage.Id}/review", new { decision = "Approved", doctorNotes = "Synthetic workflow test review", expectedUpdatedAt = forReview!.UpdatedAt });
        Assert.Equal(HttpStatusCode.OK, reviewed.StatusCode);
        var finalCase = await patient.GetFromJsonAsync<TriageResponseDto>($"/api/triage/{triage.Id}");
        var finalAppointment = await patient.GetFromJsonAsync<AppointmentDto>($"/api/Appointments/{appointmentId}");
        Assert.Equal("Approved", finalCase!.TriageStatus);
        Assert.Equal("Approved", finalCase.ApprovalStatus);
        Assert.Equal(appointmentId, finalCase.TentativeAppointmentId);
        Assert.Contains(finalCase.ReviewHistories, h => h.Action == "Approved");
        Assert.Equal("Confirmed", finalAppointment!.Status);
        Assert.Equal("Approved", finalAppointment.ApprovalStatus);
        Assert.Equal(profile.Id, finalAppointment.PatientId);
        Assert.Equal(doctor.Id, finalAppointment.ApprovedByDoctorId);
        using var verifyScope = factory.Services.CreateScope();
        var stored = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Equal(1, await stored.Appointments.CountAsync(a => a.PatientId == profile.Id));
        Assert.Equal("Confirmed", (await stored.Appointments.FindAsync(appointmentId))!.Status);
    }
}
