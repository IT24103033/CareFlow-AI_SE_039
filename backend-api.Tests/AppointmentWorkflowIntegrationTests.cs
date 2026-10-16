using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CareFlowAI.API.Data;
using CareFlowAI.API.DTOs;
using CareFlowAI.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CareFlowAI.API.Tests;

public class AppointmentWorkflowIntegrationTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AppointmentWorkflowIntegrationTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private sealed record WorkflowScenario(
        User PatientUser,
        PatientProfile Patient,
        Doctor Doctor,
        DoctorAvailability Availability);

    private async Task<WorkflowScenario> SeedWorkflowScenarioAsync()
    {
        using var scope = _factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        await db.Database.EnsureCreatedAsync();

        var suffix = Guid.NewGuid()
            .ToString("N")[..8];

        var patient = new PatientProfile
        {
            Id = Guid.NewGuid(),
            FullName = $"Workflow Patient {suffix}",
            DateOfBirth = new DateOnly(1995, 1, 1),
            BloodGroup = "O+",
            MedicalHistorySummary =
                "Appointment workflow integration test patient",
            CreatedAt = DateTime.UtcNow
        };

        var patientUser = new User
        {
            Id = Guid.NewGuid(),
            Username = $"workflow_patient_{suffix}",
            Password = "TestPassword123!",
            Role = "Patient",
            PatientProfileId = patient.Id,
            CreatedAt = DateTime.UtcNow
        };

        var doctor = new Doctor
        {
            Id = Guid.NewGuid(),
            FullName = $"Workflow Doctor {suffix}",
            Specialization = "General Medicine",
            Email = $"workflow.doctor.{suffix}@test.local",
            IsActive = true
        };

        var appointmentDate =
            DateOnly.FromDateTime(DateTime.UtcNow)
                .AddDays(14);

        var availability = new DoctorAvailability
        {
            Id = Guid.NewGuid(),
            DoctorId = doctor.Id,
            Date = appointmentDate,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(17, 0)
        };

        db.PatientProfiles.Add(patient);
        db.Users.Add(patientUser);
        db.Doctors.Add(doctor);
        db.DoctorAvailabilities.Add(availability);

        await db.SaveChangesAsync();

        return new WorkflowScenario(
            patientUser,
            patient,
            doctor,
            availability);
    }

    private string CreatePatientToken(
        WorkflowScenario scenario)
    {
        return _factory.CreateToken(
            scenario.PatientUser.Id,
            scenario.PatientUser.Username,
            "Patient",
            patientId: scenario.Patient.Id);
    }

    [Fact]
    public async Task AppointmentAgentBook_ShouldCompleteFullWorkflow_WhenSlotIsAvailable()
    {
        var scenario =
            await SeedWorkflowScenarioAsync();

        var workflowId =
            Guid.NewGuid();

        var token =
            CreatePatientToken(scenario);

        var requestBody =
            new
            {
                WorkflowId = workflowId,

                DoctorId =
                    scenario.Doctor.Id,

                // Deliberately send an incorrect patient ID.
                // The controller must ignore it and derive the
                // real patient from the authenticated user.
                PatientId =
                    Guid.NewGuid(),

                AppointmentDate =
                    scenario.Availability.Date,

                StartTime =
                    new TimeOnly(10, 0),

                EndTime =
                    new TimeOnly(10, 30)
            };

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                "/api/AppointmentAgent/book")
            {
                Content =
                    JsonContent.Create(requestBody)
            };

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        var response =
            await _client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content.ReadFromJsonAsync<AppointmentAgentResponse>();

        Assert.NotNull(result);

        Assert.True(result!.Success);

        Assert.Equal(
            "Success",
            result.Status);

        Assert.Equal(
            workflowId,
            result.WorkflowId);

        Assert.NotNull(
            result.AppointmentId);

        // Verify that the appointment actually reached the database
        // through the complete workflow.
        using var scope =
            _factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var appointment =
            await db.Appointments
                .FirstOrDefaultAsync(
                    a => a.Id == result.AppointmentId);

        Assert.NotNull(appointment);

        Assert.Equal(
            scenario.Doctor.Id,
            appointment!.DoctorId);

        Assert.Equal(
            scenario.Patient.Id,
            appointment.PatientId);

        Assert.Equal(
            scenario.Availability.Date,
            appointment.AppointmentDate);

        Assert.Equal(
            "Tentative",
            appointment.Status);

        Assert.Equal(
            "Pending",
            appointment.ApprovalStatus);
    }

    [Fact]
    public async Task AppointmentAgentBook_ShouldReturnConflict_WhenSlotAlreadyBooked()
    {
        var scenario =
            await SeedWorkflowScenarioAsync();

        var token =
            CreatePatientToken(scenario);

        using var scope =
            _factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var existingAppointment = new Appointment
        {
            Id = Guid.NewGuid(),
            DoctorId = scenario.Doctor.Id,
            PatientId = scenario.Patient.Id,
            AppointmentDate = scenario.Availability.Date,
            StartTime = new TimeOnly(11, 0),
            EndTime = new TimeOnly(11, 30),
            Status = "Tentative",
            ApprovalStatus = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        db.Appointments.Add(
            existingAppointment);

        await db.SaveChangesAsync();

        var requestBody =
            new
            {
                WorkflowId =
                    Guid.NewGuid(),

                DoctorId =
                    scenario.Doctor.Id,

                PatientId =
                    scenario.Patient.Id,

                AppointmentDate =
                    scenario.Availability.Date,

                StartTime =
                    new TimeOnly(11, 0),

                EndTime =
                    new TimeOnly(11, 30)
            };

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                "/api/AppointmentAgent/book")
            {
                Content =
                    JsonContent.Create(requestBody)
            };

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        var response =
            await _client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.Conflict,
            response.StatusCode);

        var body =
            await response.Content.ReadAsStringAsync();

        Assert.Contains(
            "Conflict",
            body,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AppointmentAgentBook_ShouldReturnUnavailable_WhenRequestedSlotIsOutsideAvailability()
    {
        var scenario =
            await SeedWorkflowScenarioAsync();

        var token =
            CreatePatientToken(scenario);

        var requestBody =
            new
            {
                WorkflowId =
                    Guid.NewGuid(),

                DoctorId =
                    scenario.Doctor.Id,

                PatientId =
                    scenario.Patient.Id,

                AppointmentDate =
                    scenario.Availability.Date,

                // Outside the 09:00–17:00 roster.
                StartTime =
                    new TimeOnly(18, 0),

                EndTime =
                    new TimeOnly(18, 30)
            };

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                "/api/AppointmentAgent/book")
            {
                Content =
                    JsonContent.Create(requestBody)
            };

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        var response =
            await _client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.Conflict,
            response.StatusCode);

        var body =
            await response.Content.ReadAsStringAsync();

        Assert.Contains(
            "Unavailable",
            body,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AppointmentAgentBook_ShouldRejectAnonymousRequest()
    {
        var response =
            await _client.PostAsJsonAsync(
                "/api/AppointmentAgent/book",
                new
                {
                    WorkflowId = Guid.NewGuid(),
                    DoctorId = Guid.NewGuid(),
                    PatientId = Guid.NewGuid(),
                    AppointmentDate =
                        DateOnly.FromDateTime(DateTime.UtcNow)
                            .AddDays(14),
                    StartTime = new TimeOnly(10, 0),
                    EndTime = new TimeOnly(10, 30)
                });

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    private sealed class AppointmentAgentResponse
    {
        public bool Success { get; set; }

        public string Status { get; set; } =
            string.Empty;

        public string Message { get; set; } =
            string.Empty;

        public Guid WorkflowId { get; set; }

        public Guid? AppointmentId { get; set; }
    }
}