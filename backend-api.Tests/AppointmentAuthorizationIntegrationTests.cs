using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CareFlowAI.API.Data;
using CareFlowAI.API.DTOs;
using CareFlowAI.API.Models;
using CareFlowAI.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CareFlowAI.API.Tests;

public class AppointmentAuthorizationIntegrationTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AppointmentAuthorizationIntegrationTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private sealed record SecurityScenario(
        User PatientUserA,
        User PatientUserB,
        User DoctorUserA,
        User DoctorUserB,
        User AdminUser,
        Doctor DoctorA,
        Doctor DoctorB,
        PatientProfile PatientA,
        PatientProfile PatientB,
        Appointment AppointmentA,
        Appointment AppointmentB);

    private async Task<SecurityScenario> SeedSecurityScenarioAsync()
    {
        using var scope = _factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        await db.Database.EnsureCreatedAsync();

        var suffix = Guid.NewGuid().ToString("N")[..8];

        // ------------------------------------------------------------
        // Patients
        // ------------------------------------------------------------

        var patientA = new PatientProfile
        {
            Id = Guid.NewGuid(),
            FullName = $"Security Patient A {suffix}",
            DateOfBirth = new DateOnly(1995, 1, 10),
            BloodGroup = "O+",
            MedicalHistorySummary = "Security integration test patient A",
            CreatedAt = DateTime.UtcNow
        };

        var patientB = new PatientProfile
        {
            Id = Guid.NewGuid(),
            FullName = $"Security Patient B {suffix}",
            DateOfBirth = new DateOnly(1996, 2, 20),
            BloodGroup = "A+",
            MedicalHistorySummary = "Security integration test patient B",
            CreatedAt = DateTime.UtcNow
        };

        // ------------------------------------------------------------
        // Doctors
        // ------------------------------------------------------------

        var doctorA = new Doctor
        {
            Id = Guid.NewGuid(),
            FullName = $"Security Doctor A {suffix}",
            Specialization = "General Medicine",
            Email = $"security.doctor.a.{suffix}@test.local",
            IsActive = true
        };

        var doctorB = new Doctor
        {
            Id = Guid.NewGuid(),
            FullName = $"Security Doctor B {suffix}",
            Specialization = "Cardiology",
            Email = $"security.doctor.b.{suffix}@test.local",
            IsActive = true
        };

        // ------------------------------------------------------------
        // Users
        // ------------------------------------------------------------

        var patientUserA = new User
        {
            Id = Guid.NewGuid(),
            Username = $"security_patient_a_{suffix}",
            Password = PasswordHasher.Hash("PatientA123!"),
            Role = "Patient",
            PatientProfileId = patientA.Id,
            CreatedAt = DateTime.UtcNow
        };

        var patientUserB = new User
        {
            Id = Guid.NewGuid(),
            Username = $"security_patient_b_{suffix}",
            Password = PasswordHasher.Hash("PatientB123!"),
            Role = "Patient",
            PatientProfileId = patientB.Id,
            CreatedAt = DateTime.UtcNow
        };

        var doctorUserA = new User
        {
            Id = Guid.NewGuid(),
            Username = $"security_doctor_a_{suffix}",
            Password = PasswordHasher.Hash("DoctorA123!"),
            Role = "Doctor",
            DoctorId = doctorA.Id,
            CreatedAt = DateTime.UtcNow
        };

        var doctorUserB = new User
        {
            Id = Guid.NewGuid(),
            Username = $"security_doctor_b_{suffix}",
            Password = PasswordHasher.Hash("DoctorB123!"),
            Role = "Doctor",
            DoctorId = doctorB.Id,
            CreatedAt = DateTime.UtcNow
        };

        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Username = $"security_admin_{suffix}",
            Password = PasswordHasher.Hash("Admin123!"),
            Role = "Admin",
            CreatedAt = DateTime.UtcNow
        };

        // ------------------------------------------------------------
        // Doctor availability
        // ------------------------------------------------------------

        var appointmentDate =
            DateOnly.FromDateTime(DateTime.UtcNow).AddDays(7);

        var availabilityA = new DoctorAvailability
        {
            Id = Guid.NewGuid(),
            DoctorId = doctorA.Id,
            Date = appointmentDate,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(17, 0)
        };

        var availabilityB = new DoctorAvailability
        {
            Id = Guid.NewGuid(),
            DoctorId = doctorB.Id,
            Date = appointmentDate,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(17, 0)
        };

        // ------------------------------------------------------------
        // Existing appointments
        // ------------------------------------------------------------

        var appointmentA = new Appointment
        {
            Id = Guid.NewGuid(),
            DoctorId = doctorA.Id,
            PatientId = patientA.Id,
            AppointmentDate = appointmentDate,
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(10, 30),
            Status = "Tentative",
            ApprovalStatus = "Pending",
            ApprovedByDoctorId = null,
            ApprovedAt = null,
            CreatedAt = DateTime.UtcNow
        };

        var appointmentB = new Appointment
        {
            Id = Guid.NewGuid(),
            DoctorId = doctorB.Id,
            PatientId = patientB.Id,
            AppointmentDate = appointmentDate,
            StartTime = new TimeOnly(11, 0),
            EndTime = new TimeOnly(11, 30),
            Status = "Tentative",
            ApprovalStatus = "Pending",
            ApprovedByDoctorId = null,
            ApprovedAt = null,
            CreatedAt = DateTime.UtcNow
        };

        db.PatientProfiles.AddRange(
            patientA,
            patientB);

        db.Doctors.AddRange(
            doctorA,
            doctorB);

        db.Users.AddRange(
            patientUserA,
            patientUserB,
            doctorUserA,
            doctorUserB,
            adminUser);

        db.DoctorAvailabilities.AddRange(
            availabilityA,
            availabilityB);

        db.Appointments.AddRange(
            appointmentA,
            appointmentB);

        await db.SaveChangesAsync();

        return new SecurityScenario(
            patientUserA,
            patientUserB,
            doctorUserA,
            doctorUserB,
            adminUser,
            doctorA,
            doctorB,
            patientA,
            patientB,
            appointmentA,
            appointmentB);
    }

    private void AddBearerToken(
        HttpRequestMessage request,
        string token)
    {
        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);
    }

    [Fact]
    public async Task Anonymous_GetAppointments_Returns401Unauthorized()
    {
        var response =
            await _client.GetAsync("/api/Appointments");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task Admin_GetAllAppointments_Returns200Ok()
    {
        var scenario =
            await SeedSecurityScenarioAsync();

        var token = _factory.CreateToken(
            scenario.AdminUser.Id,
            scenario.AdminUser.Username,
            "Admin");

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                "/api/Appointments");

        AddBearerToken(request, token);

        var response =
            await _client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }

    [Fact]
    public async Task Patient_CanViewOwnAppointment_Returns200Ok()
    {
        var scenario =
            await SeedSecurityScenarioAsync();

        var token = _factory.CreateToken(
            scenario.PatientUserA.Id,
            scenario.PatientUserA.Username,
            "Patient",
            patientId: scenario.PatientA.Id);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                $"/api/Appointments/{scenario.AppointmentA.Id}");

        AddBearerToken(request, token);

        var response =
            await _client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }

    [Fact]
    public async Task Patient_CannotViewAnotherPatientsAppointment_Returns403Forbidden()
    {
        var scenario =
            await SeedSecurityScenarioAsync();

        var token = _factory.CreateToken(
            scenario.PatientUserA.Id,
            scenario.PatientUserA.Username,
            "Patient",
            patientId: scenario.PatientA.Id);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                $"/api/Appointments/{scenario.AppointmentB.Id}");

        AddBearerToken(request, token);

        var response =
            await _client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    [Fact]
    public async Task Patient_CannotAccessAnotherPatientsAppointmentList_Returns403Forbidden()
    {
        var scenario =
            await SeedSecurityScenarioAsync();

        var token = _factory.CreateToken(
            scenario.PatientUserA.Id,
            scenario.PatientUserA.Username,
            "Patient",
            patientId: scenario.PatientA.Id);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                $"/api/Appointments/patient/{scenario.PatientB.Id}");

        AddBearerToken(request, token);

        var response =
            await _client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    [Fact]
    public async Task Patient_CannotConfirmAppointment_Returns403Forbidden()
    {
        var scenario =
            await SeedSecurityScenarioAsync();

        var token = _factory.CreateToken(
            scenario.PatientUserA.Id,
            scenario.PatientUserA.Username,
            "Patient",
            patientId: scenario.PatientA.Id);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                $"/api/Appointments/{scenario.AppointmentA.Id}/confirm");

        AddBearerToken(request, token);

        var response =
            await _client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    [Fact]
    public async Task Doctor_CannotApproveAppointmentAssignedToAnotherDoctor_Returns403Forbidden()
    {
        var scenario =
            await SeedSecurityScenarioAsync();

        var token = _factory.CreateToken(
            scenario.DoctorUserB.Id,
            scenario.DoctorUserB.Username,
            "Doctor",
            doctorId: scenario.DoctorB.Id);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                $"/api/Appointments/{scenario.AppointmentA.Id}/approve");

        AddBearerToken(request, token);

        var response =
            await _client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    [Fact]
    public async Task Doctor_CanApproveOwnAppointment_Returns200Ok()
    {
        var scenario =
            await SeedSecurityScenarioAsync();

        var token = _factory.CreateToken(
            scenario.DoctorUserA.Id,
            scenario.DoctorUserA.Username,
            "Doctor",
            doctorId: scenario.DoctorA.Id);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                $"/api/Appointments/{scenario.AppointmentA.Id}/approve");

        AddBearerToken(request, token);

        var response =
            await _client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var approved =
            await response.Content
                .ReadFromJsonAsync<AppointmentDto>();

        Assert.NotNull(approved);

        Assert.Equal(
            "Approved",
            approved!.ApprovalStatus);

        Assert.Equal(
            scenario.DoctorA.Id,
            approved.ApprovedByDoctorId);
    }

    [Fact]
    public async Task Patient_TentativeBooking_DerivesPatientIdentityFromAuthenticatedUser()
    {
        var scenario =
            await SeedSecurityScenarioAsync();

        var token = _factory.CreateToken(
            scenario.PatientUserA.Id,
            scenario.PatientUserA.Username,
            "Patient",
            patientId: scenario.PatientA.Id);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                "/api/Appointments/tentative")
            {
                Content =
                    JsonContent.Create(
                        new CreateAppointmentDto
                        {
                            DoctorId =
                                scenario.DoctorA.Id,

                            // Deliberately submit Patient B.
                            // The controller must ignore this value
                            // and use the authenticated Patient A.
                            PatientId =
                                scenario.PatientB.Id,

                            AppointmentDate =
                                scenario.AppointmentA.AppointmentDate,

                            StartTime =
                                new TimeOnly(13, 0),

                            EndTime =
                                new TimeOnly(13, 30)
                        })
            };

        AddBearerToken(request, token);

        var response =
            await _client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        var created =
            await response.Content
                .ReadFromJsonAsync<AppointmentDto>();

        Assert.NotNull(created);

        Assert.Equal(
            scenario.PatientA.Id,
            created!.PatientId);

        Assert.NotEqual(
            scenario.PatientB.Id,
            created.PatientId);
    }
}