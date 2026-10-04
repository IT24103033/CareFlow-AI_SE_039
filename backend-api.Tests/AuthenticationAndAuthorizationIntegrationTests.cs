using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using CareFlowAI.API.Data;
using CareFlowAI.API.DTOs;
using CareFlowAI.API.Models;
using CareFlowAI.API.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace CareFlowAI.API.Tests;

public class TestAssessmentClient : IClinicalAssessmentClient
{
    private const string Assessment = """
        {"SuggestedSpecialist":"General Practitioner","UrgencyLevel":"Medium","RecommendedAction":"Review the patient context.","Rationale":"Symptoms require clinician review."}
        """;

    public Task<string> AssessAsync(ClinicalAssessmentInput input, CancellationToken cancellationToken)
    {
        return Task.FromResult(Assessment);
    }
}

public class TestNotificationService : INotificationService
{
    public Task<bool> SendEmailAsync(string toEmail, string subject, string messageBody) => Task.FromResult(true);
    public Task<bool> SendSmsAsync(string phoneNumber, string message) => Task.FromResult(true);
    public Task<(bool Success, string? FailureReason)> DispatchPrescriptionNotificationAsync(
        string patientName,
        string? patientEmail,
        string? patientPhone,
        string prescriptionSummary,
        string channel = "Both")
    {
        return Task.FromResult((true, (string?)null));
    }
}

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public readonly string DatabaseName = Guid.NewGuid().ToString();
    public const string TestSecret = "CareFlowAI_Super_Secret_Key_For_Jwt_Signing_Must_Be_Long_Enough_2026!";
    public const string TestIssuer = "CareFlowAI";
    public const string TestAudience = "CareFlowAI.Clients";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = TestSecret,
                ["Jwt:Issuer"] = TestIssuer,
                ["Jwt:Audience"] = TestAudience,
                ["Jwt:ExpiryMinutes"] = "120"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Replace DbContext with in-memory database
            var dbDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (dbDescriptor != null) services.Remove(dbDescriptor);

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase(DatabaseName)
                       .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning));
            });

            // Replace assessment client stub
            var clientDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IClinicalAssessmentClient));
            if (clientDescriptor != null) services.Remove(clientDescriptor);
            services.AddScoped<IClinicalAssessmentClient, TestAssessmentClient>();

            // Replace notification service stub
            var notifDescriptors = services.Where(d => d.ServiceType == typeof(INotificationService)).ToList();
            foreach (var d in notifDescriptors) services.Remove(d);
            // Also remove TypedHttpClient registrations for NotificationService
            var httpClientDescriptors = services
                .Where(d => d.ImplementationType == typeof(NotificationService))
                .ToList();
            foreach (var d in httpClientDescriptors) services.Remove(d);
            services.AddScoped<INotificationService, TestNotificationService>();

            // ISafetyAgent is satisfied by PharmacyAiService – no stub needed; leave as-is.
        });
    }

    public string CreateToken(Guid userId, string username, string role, Guid? doctorId = null, Guid? patientId = null, DateTime? expires = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new("sub", userId.ToString()),
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Role, role),
            new("role", role)
        };

        if (doctorId.HasValue) claims.Add(new("doctor_id", doctorId.Value.ToString()));
        if (patientId.HasValue) claims.Add(new("patient_id", patientId.Value.ToString()));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: TestIssuer,
            audience: TestAudience,
            claims: claims,
            expires: expires ?? DateTime.UtcNow.AddHours(2),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class AuthenticationAndAuthorizationIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthenticationAndAuthorizationIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<(User User, Doctor Doctor, PatientProfile PatientA, PatientProfile PatientB)> SeedDataAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Ensure database is created and seeded
        await context.Database.EnsureCreatedAsync();

        var doctor = await context.Doctors.FirstOrDefaultAsync(d => d.Email == "active.doctor@hospital.org");
        if (doctor == null)
        {
            doctor = new Doctor
            {
                Id = Guid.NewGuid(),
                FullName = "Dr. Alice Smith",
                Specialization = "Emergency Medicine",
                Email = "active.doctor@hospital.org",
                IsActive = true
            };
            context.Doctors.Add(doctor);
        }

        var doctorUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "doctor_alice");
        if (doctorUser == null)
        {
            doctorUser = new User
            {
                Id = Guid.NewGuid(),
                Username = "doctor_alice",
                Password = PasswordHasher.Hash("DocSecurePass123!"),
                Role = "Doctor",
                DoctorId = doctor.Id,
                CreatedAt = DateTime.UtcNow
            };
            context.Users.Add(doctorUser);
        }

        var patientA = await context.PatientProfiles.FirstOrDefaultAsync(p => p.FullName == "Patient Alpha");
        if (patientA == null)
        {
            patientA = new PatientProfile
            {
                Id = Guid.NewGuid(),
                FullName = "Patient Alpha",
                DateOfBirth = DateOnly.Parse("1990-01-01"),
                BloodGroup = "O+",
                MedicalHistorySummary = "Asthma",
                CreatedAt = DateTime.UtcNow
            };
            context.PatientProfiles.Add(patientA);
        }

        var userPatientA = await context.Users.FirstOrDefaultAsync(u => u.Username == "patient_alpha");
        if (userPatientA == null)
        {
            userPatientA = new User
            {
                Id = Guid.NewGuid(),
                Username = "patient_alpha",
                Password = PasswordHasher.Hash("PatientPass123!"),
                Role = "Patient",
                PatientProfileId = patientA.Id,
                CreatedAt = DateTime.UtcNow
            };
            context.Users.Add(userPatientA);
        }

        var patientB = await context.PatientProfiles.FirstOrDefaultAsync(p => p.FullName == "Patient Beta");
        if (patientB == null)
        {
            patientB = new PatientProfile
            {
                Id = Guid.NewGuid(),
                FullName = "Patient Beta",
                DateOfBirth = DateOnly.Parse("1992-05-15"),
                BloodGroup = "A+",
                MedicalHistorySummary = "None",
                CreatedAt = DateTime.UtcNow
            };
            context.PatientProfiles.Add(patientB);
        }

        var userPatientB = await context.Users.FirstOrDefaultAsync(u => u.Username == "patient_beta");
        if (userPatientB == null)
        {
            userPatientB = new User
            {
                Id = Guid.NewGuid(),
                Username = "patient_beta",
                Password = PasswordHasher.Hash("PatientBetaPass123!"),
                Role = "Patient",
                PatientProfileId = patientB.Id,
                CreatedAt = DateTime.UtcNow
            };
            context.Users.Add(userPatientB);
        }

        var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "admin_root");
        if (adminUser == null)
        {
            adminUser = new User
            {
                Id = Guid.NewGuid(),
                Username = "admin_root",
                Password = PasswordHasher.Hash("AdminPass123!"),
                Role = "Admin",
                CreatedAt = DateTime.UtcNow
            };
            context.Users.Add(adminUser);
        }

        await context.SaveChangesAsync();
        return (doctorUser, doctor, patientA, patientB);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_Returns401Unauthorized()
    {
        await SeedDataAsync();

        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto
        {
            Username = "doctor_alice",
            Password = "WrongPassword999!"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidCredentials_Returns200AndValidJwt()
    {
        await SeedDataAsync();

        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto
        {
            Username = "doctor_alice",
            Password = "DocSecurePass123!"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var authResult = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(authResult);
        Assert.False(string.IsNullOrWhiteSpace(authResult.Token));
        Assert.Equal("Doctor", authResult.User.Role);
        Assert.NotNull(authResult.User.DoctorId);
    }

    [Fact]
    public async Task AnonymousRequest_ToProtectedEndpoints_Returns401Unauthorized()
    {
        using var client = _factory.CreateClient(); // client without headers

        var meRes = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, meRes.StatusCode);

        var queueRes = await client.GetAsync("/api/triage/review-queue");
        Assert.Equal(HttpStatusCode.Unauthorized, queueRes.StatusCode);

        var patientsRes = await client.GetAsync("/api/patientprofiles");
        Assert.Equal(HttpStatusCode.Unauthorized, patientsRes.StatusCode);

        var pxRes = await client.GetAsync("/api/prescriptions");
        Assert.Equal(HttpStatusCode.Unauthorized, pxRes.StatusCode);
    }

    [Fact]
    public async Task TamperedToken_OrExpiredToken_Returns401Unauthorized()
    {
        var (_, _, patientA, _) = await SeedDataAsync();

        // 1. Expired token
        var expiredToken = _factory.CreateToken(
            Guid.NewGuid(), "expired_user", "Patient",
            patientId: patientA.Id,
            expires: DateTime.UtcNow.AddMinutes(-10)
        );

        using var request1 = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        request1.Headers.Authorization = new AuthenticationHeaderValue("Bearer", expiredToken);
        var response1 = await _client.SendAsync(request1);
        Assert.Equal(HttpStatusCode.Unauthorized, response1.StatusCode);

        // 2. Tampered signature
        var validToken = _factory.CreateToken(Guid.NewGuid(), "patient_alpha", "Patient", patientId: patientA.Id);
        var tamperedToken = validToken[..^6] + "xxxxxx";

        using var request2 = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        request2.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tamperedToken);
        var response2 = await _client.SendAsync(request2);
        Assert.Equal(HttpStatusCode.Unauthorized, response2.StatusCode);
    }

    [Fact]
    public async Task RoleBasedAccess_DoctorQueue_OnlyDoctorCanAccess()
    {
        var (_, doctor, patientA, _) = await SeedDataAsync();

        // Patient token
        var patientToken = _factory.CreateToken(Guid.NewGuid(), "patient_alpha", "Patient", patientId: patientA.Id);
        using var patientReq = new HttpRequestMessage(HttpMethod.Get, "/api/triage/review-queue");
        patientReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", patientToken);
        var patientRes = await _client.SendAsync(patientReq);
        Assert.Equal(HttpStatusCode.Forbidden, patientRes.StatusCode);

        // Staff token
        var staffToken = _factory.CreateToken(Guid.NewGuid(), "staff_member", "Staff");
        using var staffReq = new HttpRequestMessage(HttpMethod.Get, "/api/triage/review-queue");
        staffReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", staffToken);
        var staffRes = await _client.SendAsync(staffReq);
        Assert.Equal(HttpStatusCode.Forbidden, staffRes.StatusCode);

        // Doctor token with active doctor id
        var doctorToken = _factory.CreateToken(Guid.NewGuid(), "doctor_alice", "Doctor", doctorId: doctor.Id);
        using var doctorReq = new HttpRequestMessage(HttpMethod.Get, "/api/triage/review-queue");
        doctorReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", doctorToken);
        var doctorRes = await _client.SendAsync(doctorReq);
        Assert.Equal(HttpStatusCode.OK, doctorRes.StatusCode);
    }

    [Fact]
    public async Task DoctorReview_WithInactiveOrMissingDoctorId_Returns403Forbidden()
    {
        var (_, _, patientA, _) = await SeedDataAsync();

        // Create a triage record in review state
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var record = new TriageRecord
        {
            Id = Guid.NewGuid(),
            PatientId = patientA.Id,
            Symptoms = "Patient reports persistent chest discomfort and shortness of breath.",
            SeverityLevel = "Medium",
            TriageStatus = "InReview",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var agent = new AgentWorkflowState
        {
            Id = Guid.NewGuid(),
            TriageRecordId = record.Id,
            AgentName = "PlanningAgent",
            AgentStatus = "Completed",
            ApprovalStatus = "Pending",
            OutputPayload = """
                {"PlanSummary":"Plan details","AnalysisMethod":"Standard"}
                """,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = record.UpdatedAt
        };
        context.TriageRecords.Add(record);
        context.AgentWorkflows.Add(agent);
        await context.SaveChangesAsync();

        // Doctor token with non-existent doctor_id
        var fakeDoctorToken = _factory.CreateToken(Guid.NewGuid(), "fake_doc", "Doctor", doctorId: Guid.NewGuid());
        using var req = new HttpRequestMessage(HttpMethod.Patch, $"/api/triage/{record.Id}/review")
        {
            Content = JsonContent.Create(new ReviewTriageDto
            {
                Decision = "Approved",
                ExpectedUpdatedAt = record.UpdatedAt,
                DoctorNotes = "All clear"
            })
        };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", fakeDoctorToken);
        var res = await _client.SendAsync(req);
        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
    }

    [Fact]
    public async Task PatientIsolation_CannotSubmitOnBehalfOfOtherPatient()
    {
        var (_, _, patientA, patientB) = await SeedDataAsync();

        var patientAToken = _factory.CreateToken(Guid.NewGuid(), "patient_alpha", "Patient", patientId: patientA.Id);

        // Patient A attempts to submit a triage record on behalf of Patient B
        using var req = new HttpRequestMessage(HttpMethod.Post, "/api/triage")
        {
            Content = JsonContent.Create(new CreateTriageRequestDto
            {
                PatientId = patientB.Id,
                Symptoms = "Severe headache and dizziness reported for several days."
            })
        };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", patientAToken);
        var res = await _client.SendAsync(req);

        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
    }

    [Fact]
    public async Task PatientIsolation_SubmittingWithOwnId_SucceedsAndDerivesIdentity()
    {
        var (_, _, patientA, _) = await SeedDataAsync();

        var patientAToken = _factory.CreateToken(Guid.NewGuid(), "patient_alpha", "Patient", patientId: patientA.Id);

        using var req = new HttpRequestMessage(HttpMethod.Post, "/api/triage")
        {
            Content = JsonContent.Create(new CreateTriageRequestDto
            {
                PatientId = patientA.Id,
                Symptoms = "Recurring high fever and persistent dry cough for 3 days."
            })
        };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", patientAToken);
        var res = await _client.SendAsync(req);

        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
        var dto = await res.Content.ReadFromJsonAsync<TriageResponseDto>();
        Assert.NotNull(dto);
        Assert.Equal(patientA.Id, dto.PatientId);
    }

    [Fact]
    public async Task PatientIsolation_CannotViewOtherPatientRecord()
    {
        var (_, _, patientA, patientB) = await SeedDataAsync();

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var recordB = new TriageRecord
        {
            Id = Guid.NewGuid(),
            PatientId = patientB.Id,
            Symptoms = "Patient Beta private symptom details reported confidentially.",
            SeverityLevel = "Low",
            TriageStatus = "Pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.TriageRecords.Add(recordB);
        await context.SaveChangesAsync();

        var patientAToken = _factory.CreateToken(Guid.NewGuid(), "patient_alpha", "Patient", patientId: patientA.Id);

        // Patient A tries to GET Patient B's record
        using var req = new HttpRequestMessage(HttpMethod.Get, $"/api/triage/{recordB.Id}");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", patientAToken);
        var res = await _client.SendAsync(req);

        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
    }

    [Fact]
    public async Task PatientIsolation_GetTriageList_OnlyReturnsOwnRecords()
    {
        var (_, _, patientA, patientB) = await SeedDataAsync();

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var recordA = new TriageRecord
        {
            Id = Guid.NewGuid(),
            PatientId = patientA.Id,
            Symptoms = "Patient Alpha symptoms for list test verification.",
            SeverityLevel = "Low",
            TriageStatus = "Pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var recordB = new TriageRecord
        {
            Id = Guid.NewGuid(),
            PatientId = patientB.Id,
            Symptoms = "Patient Beta symptoms for list test verification.",
            SeverityLevel = "Medium",
            TriageStatus = "Pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.TriageRecords.AddRange(recordA, recordB);
        await context.SaveChangesAsync();

        var patientAToken = _factory.CreateToken(Guid.NewGuid(), "patient_alpha", "Patient", patientId: patientA.Id);

        using var req = new HttpRequestMessage(HttpMethod.Get, "/api/triage");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", patientAToken);
        var res = await _client.SendAsync(req);

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var list = await res.Content.ReadFromJsonAsync<List<TriageResponseDto>>();
        Assert.NotNull(list);
        Assert.All(list, r => Assert.Equal(patientA.Id, r.PatientId));
    }

    [Fact]
    public async Task PublicRegistration_EnforcesPatientRole_RoleEscalationPrevented()
    {
        var randomUser = $"user_{Guid.NewGuid().ToString("N")[..8]}";
        var randomEmail = $"{randomUser}@example.com";

        var response = await _client.PostAsJsonAsync("/api/auth/register-patient", new RegisterPatientRequestDto
        {
            Username = randomUser,
            Email = randomEmail,
            Password = "SecurePass1234!",
            FullName = "Self Registered Patient",
            DateOfBirth = DateOnly.Parse("1995-10-10")
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var authRes = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(authRes);
        Assert.Equal("Patient", authRes.User.Role);
        Assert.Equal(randomEmail, authRes.User.Email);

        // Verify in database that Role is strictly 'Patient' and Email is stored
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userInDb = await context.Users.SingleAsync(u => u.Username == randomUser);
        Assert.Equal("Patient", userInDb.Role);
        Assert.Equal(randomEmail, userInDb.Email);
    }

    [Fact]
    public async Task PublicRegistration_AllowsLoginWithEmail()
    {
        var randomUser = $"user_{Guid.NewGuid().ToString("N")[..8]}";
        var randomEmail = $"{randomUser}@example.com";
        var password = "SecurePass1234!";

        var regRes = await _client.PostAsJsonAsync("/api/auth/register-patient", new RegisterPatientRequestDto
        {
            Username = randomUser,
            Email = randomEmail,
            Password = password,
            FullName = "Email Login Patient",
            DateOfBirth = DateOnly.Parse("1992-05-15")
        });
        Assert.Equal(HttpStatusCode.Created, regRes.StatusCode);

        // Log in using email instead of username
        var loginRes = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto
        {
            Username = randomEmail,
            Password = password
        });

        Assert.Equal(HttpStatusCode.OK, loginRes.StatusCode);
        var authRes = await loginRes.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(authRes);
        Assert.Equal(randomUser, authRes.User.Username);
        Assert.Equal(randomEmail, authRes.User.Email);
        Assert.Equal("Patient", authRes.User.Role);
    }

    [Fact]
    public async Task PublicRegistration_DuplicateEmail_Returns409Conflict()
    {
        var email = $"duplicate_{Guid.NewGuid().ToString("N")[..6]}@example.com";

        var res1 = await _client.PostAsJsonAsync("/api/auth/register-patient", new RegisterPatientRequestDto
        {
            Username = $"user1_{Guid.NewGuid().ToString("N")[..6]}",
            Email = email,
            Password = "Password123!",
            FullName = "First User",
            DateOfBirth = DateOnly.Parse("1990-01-01")
        });
        Assert.Equal(HttpStatusCode.Created, res1.StatusCode);

        var res2 = await _client.PostAsJsonAsync("/api/auth/register-patient", new RegisterPatientRequestDto
        {
            Username = $"user2_{Guid.NewGuid().ToString("N")[..6]}",
            Email = email,
            Password = "Password123!",
            FullName = "Second User",
            DateOfBirth = DateOnly.Parse("1991-02-02")
        });
        Assert.Equal(HttpStatusCode.Conflict, res2.StatusCode);
    }

    [Fact]
    public async Task RegisterStaff_ByNonAdmin_Returns403Or401()
    {
        var (_, _, patientA, _) = await SeedDataAsync();

        // 1. Anonymous attempt -> 401
        var anonRes = await _client.PostAsJsonAsync("/api/auth/register-staff", new RegisterStaffRequestDto
        {
            Username = "fake_admin",
            Password = "Password123!",
            Role = "Admin"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, anonRes.StatusCode);

        // 2. Patient attempt -> 403
        var patientToken = _factory.CreateToken(Guid.NewGuid(), "patient_alpha", "Patient", patientId: patientA.Id);
        using var patientReq = new HttpRequestMessage(HttpMethod.Post, "/api/auth/register-staff")
        {
            Content = JsonContent.Create(new RegisterStaffRequestDto
            {
                Username = "escalated_user",
                Password = "Password123!",
                Role = "Admin"
            })
        };
        patientReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", patientToken);
        var patientRes = await _client.SendAsync(patientReq);
        Assert.Equal(HttpStatusCode.Forbidden, patientRes.StatusCode);

        // 3. Admin attempt -> 201 Created
        var adminToken = _factory.CreateToken(Guid.NewGuid(), "admin_root", "Admin");
        var newStaffName = $"staff_{Guid.NewGuid().ToString("N")[..6]}";
        using var adminReq = new HttpRequestMessage(HttpMethod.Post, "/api/auth/register-staff")
        {
            Content = JsonContent.Create(new RegisterStaffRequestDto
            {
                Username = newStaffName,
                Password = "StaffPassword123!",
                Role = "Staff",
                FullName = "Hospital Nurse"
            })
        };
        adminReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var adminRes = await _client.SendAsync(adminReq);
        Assert.Equal(HttpStatusCode.Created, adminRes.StatusCode);
    }
}
