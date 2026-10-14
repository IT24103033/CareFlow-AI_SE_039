using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CareFlowAI.API.Controllers;
using CareFlowAI.API.Data;
using CareFlowAI.API.DTOs;
using CareFlowAI.API.Models;
using CareFlowAI.API.Services.Notifications;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace CareFlowAI.API.Tests;

public class NotificationIntegrationTests
{
    private static ApplicationDbContext CreateDb() => new(
        new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private sealed class SimpleOptionsMonitor<T> : IOptionsMonitor<T>
    {
        public SimpleOptionsMonitor(T currentValue) => CurrentValue = currentValue;
        public T CurrentValue { get; set; }
        public T Get(string? name) => CurrentValue;
        public IDisposable? OnChange(Action<T, string?> listener) => null;
    }

    private sealed class MockEmailProvider : IEmailProvider
    {
        public bool IsConfigured { get; set; } = true;
        public int SendCount { get; private set; }
        public EmailMessage? LastMessage { get; private set; }
        public EmailSendResult ResultToReturn { get; set; } =
            new(EmailSendOutcome.Accepted, "msg_test_accepted_123");

        public Task<EmailSendResult> SendAsync(EmailMessage message, CancellationToken cancellationToken)
        {
            SendCount++;
            LastMessage = message;
            return Task.FromResult(ResultToReturn);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 1. Recipient Resolver Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task RecipientResolver_ResolvesUserEmail_LinkedByPatientProfileId()
    {
        using var db = CreateDb();
        var patient = new PatientProfile
        {
            FullName = "John Linked",
            Email = "profile.email@patientmail.com" // profile has fallback
        };
        db.PatientProfiles.Add(patient);

        var user = new User
        {
            Username = "john_linked",
            Email = "primary.account@patientmail.com",
            Password = "hashedpassword",
            Role = "Patient",
            PatientProfileId = patient.Id
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var resolver = new NotificationRecipientResolver(db);
        var result = await resolver.ResolveEmailAsync(patient.Id);

        Assert.True(result.Ok);
        // Primary user account email linked via PatientProfileId takes precedence
        Assert.Equal("primary.account@patientmail.com", result.Email);
    }

    [Fact]
    public async Task RecipientResolver_FallsBackToPatientProfileEmail_WhenNoUserLinked()
    {
        using var db = CreateDb();
        var patient = new PatientProfile
        {
            FullName = "Jane Standalone",
            Email = "jane.standalone@patientmail.com"
        };
        db.PatientProfiles.Add(patient);
        await db.SaveChangesAsync();

        var resolver = new NotificationRecipientResolver(db);
        var result = await resolver.ResolveEmailAsync(patient.Id);

        Assert.True(result.Ok);
        Assert.Equal("jane.standalone@patientmail.com", result.Email);
    }

    [Fact]
    public async Task RecipientResolver_NeverMatchesByPatientName()
    {
        using var db = CreateDb();
        var patient = new PatientProfile
        {
            FullName = "Alice Smith",
            Email = null // no email on profile
        };
        db.PatientProfiles.Add(patient);

        // A user account with identical name, but NOT linked by PatientProfileId
        var unlinkedUser = new User
        {
            Username = "alice_smith",
            Email = "alice.unlinked@patientmail.com",
            Password = "hashedpassword",
            Role = "Patient",
            PatientProfileId = null // Not linked
        };
        db.Users.Add(unlinkedUser);
        await db.SaveChangesAsync();

        var resolver = new NotificationRecipientResolver(db);
        var result = await resolver.ResolveEmailAsync(patient.Id);

        // Must NOT match by name: must fail cleanly with NoRecipient
        Assert.False(result.Ok);
        Assert.Equal(NotificationFailureCodes.NoRecipient, result.FailureCode);
    }

    [Fact]
    public async Task RecipientResolver_BlocksPlaceholderDomains()
    {
        using var db = CreateDb();
        var patient = new PatientProfile
        {
            FullName = "Dummy User",
            Email = "dummy@careflow.ai" // internal/placeholder domain
        };
        db.PatientProfiles.Add(patient);
        await db.SaveChangesAsync();

        var resolver = new NotificationRecipientResolver(db);
        var result = await resolver.ResolveEmailAsync(patient.Id);

        Assert.False(result.Ok);
        Assert.Equal(NotificationFailureCodes.InvalidRecipient, result.FailureCode);
    }

    [Fact]
    public async Task RecipientResolver_ReturnsNoRecipient_WhenEmpty()
    {
        using var db = CreateDb();
        var patient = new PatientProfile
        {
            FullName = "Empty Patient",
            Email = "   "
        };
        db.PatientProfiles.Add(patient);
        await db.SaveChangesAsync();

        var resolver = new NotificationRecipientResolver(db);
        var result = await resolver.ResolveEmailAsync(patient.Id);

        Assert.False(result.Ok);
        Assert.Equal(NotificationFailureCodes.NoRecipient, result.FailureCode);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 2. Content Builder & Privacy Compliance Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void ContentBuilder_PrescriptionIssued_DoesNotContainMedicalOrMedicationDetails()
    {
        var content = NotificationContentBuilder.PrescriptionIssued("John Doe", "https://app.careflow.com");

        Assert.Contains("Hello John", content.Html);
        Assert.Contains("open the CareFlow app", content.Html);
        Assert.Contains("https://app.careflow.com", content.Html);

        // Sensitive keywords that must NEVER be in outbound email
        Assert.DoesNotContain("prescription", content.Subject, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("medication", content.Html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("diagnosis", content.Html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("mg", content.Html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ContentBuilder_AppointmentConfirmed_IncludesDateAndDoctor_NoMedicalDetails()
    {
        var date = new DateOnly(2026, 11, 20);
        var time = new TimeOnly(14, 30);
        var content = NotificationContentBuilder.AppointmentConfirmed("Sarah Jenkins", date, time, "Dr. Emily House", "https://app.careflow.com");

        Assert.Contains("Hello Sarah", content.Html);
        Assert.Contains("Dr. Emily House", content.Html);
        Assert.Contains("20 November 2026", content.Html);
        Assert.Contains("14:30", content.Html);

        // Privacy checks
        Assert.DoesNotContain("symptoms", content.Html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("chest pain", content.Html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("disease", content.Html, StringComparison.OrdinalIgnoreCase);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 3. Resend Provider Tests (with Mock HttpHandler)
    // ─────────────────────────────────────────────────────────────────────────

    private sealed class DelegatingMockHandler : HttpMessageHandler
    {
        public Func<HttpRequestMessage, Task<HttpResponseMessage>> Handler { get; set; } =
            _ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Handler(request);
    }

    [Fact]
    public async Task ResendEmailProvider_WhenNotConfigured_ReturnsNotConfiguredOutcome()
    {
        var opts = new NotificationOptions { Email = new EmailOptions { ApiKey = "", From = "" } };
        var monitor = new SimpleOptionsMonitor<NotificationOptions>(opts);
        var provider = new ResendEmailProvider(new HttpClient(), monitor, NullLogger<ResendEmailProvider>.Instance);

        Assert.False(provider.IsConfigured);
        var result = await provider.SendAsync(new EmailMessage("test@patientmail.com", "Sub", "<p>hi</p>", "hi", "key-1"), CancellationToken.None);

        Assert.Equal(EmailSendOutcome.NotConfigured, result.Outcome);
    }

    [Fact]
    public async Task ResendEmailProvider_SendsCorrectHeadersAndIdempotencyKey()
    {
        HttpRequestMessage? capturedReq = null;
        string? capturedBody = null;

        var handler = new DelegatingMockHandler
        {
            Handler = async req =>
            {
                capturedReq = req;
                capturedBody = req.Content != null ? await req.Content.ReadAsStringAsync() : null;
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"id\":\"re_msg_abc123\"}", Encoding.UTF8, "application/json")
                };
            }
        };

        var opts = new NotificationOptions
        {
            Email = new EmailOptions
            {
                Provider = "Resend",
                ApiKey = "re_test_key_valid",
                From = "CareFlow <notifications@careflowhospital.org>"
            }
        };
        var monitor = new SimpleOptionsMonitor<NotificationOptions>(opts);
        var provider = new ResendEmailProvider(new HttpClient(handler), monitor, NullLogger<ResendEmailProvider>.Instance);

        var message = new EmailMessage("patient@patientmail.com", "Test Subject", "<p>Hello</p>", "Hello", "careflow-idemp-key-999");
        var result = await provider.SendAsync(message, CancellationToken.None);

        Assert.Equal(EmailSendOutcome.Accepted, result.Outcome);
        Assert.Equal("re_msg_abc123", result.ProviderMessageId);

        Assert.NotNull(capturedReq);
        Assert.Equal("Bearer", capturedReq!.Headers.Authorization?.Scheme);
        Assert.Equal("re_test_key_valid", capturedReq.Headers.Authorization?.Parameter);
        Assert.True(capturedReq.Headers.Contains("Idempotency-Key"));
        Assert.Equal("careflow-idemp-key-999", capturedReq.Headers.GetValues("Idempotency-Key").FirstOrDefault());

        Assert.NotNull(capturedBody);
        using var json = JsonDocument.Parse(capturedBody!);
        Assert.Equal("CareFlow <notifications@careflowhospital.org>", json.RootElement.GetProperty("from").GetString());
        Assert.Equal("patient@patientmail.com", json.RootElement.GetProperty("to")[0].GetString());
        Assert.Equal("Test Subject", json.RootElement.GetProperty("subject").GetString());
    }

    [Fact]
    public async Task ResendEmailProvider_Handles429RateLimit_Gracefully()
    {
        var handler = new DelegatingMockHandler
        {
            Handler = _ =>
            {
                var resp = new HttpResponseMessage((HttpStatusCode)429)
                {
                    Content = new StringContent("{\"message\":\"Too many requests\"}", Encoding.UTF8, "application/json")
                };
                resp.Headers.Add("Retry-After", "30");
                return Task.FromResult(resp);
            }
        };

        var opts = new NotificationOptions
        {
            Email = new EmailOptions { ApiKey = "re_valid", From = "CareFlow <care@hospital.org>" }
        };
        var monitor = new SimpleOptionsMonitor<NotificationOptions>(opts);
        var provider = new ResendEmailProvider(new HttpClient(handler), monitor, NullLogger<ResendEmailProvider>.Instance);

        var result = await provider.SendAsync(
            new EmailMessage("patient@patientmail.com", "Sub", "<p>hi</p>", "hi", "key-1"),
            CancellationToken.None);

        Assert.Equal(EmailSendOutcome.RateLimited, result.Outcome);
        Assert.NotNull(result.FailureReason);
        Assert.Contains("rate limit", result.FailureReason, StringComparison.OrdinalIgnoreCase);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 4. Dispatcher Outbox & State Management Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Dispatcher_Enqueue_IsIdempotent()
    {
        using var db = CreateDb();
        var patientId = Guid.NewGuid();
        var prescriptionId = Guid.NewGuid();

        var opts = new NotificationOptions();
        var monitor = new SimpleOptionsMonitor<NotificationOptions>(opts);
        var resolver = new NotificationRecipientResolver(db);
        var dispatcher = new NotificationDispatcher(db, new MockEmailProvider(), resolver, monitor, NullLogger<NotificationDispatcher>.Instance);

        var first = await dispatcher.EnqueueAsync(NotificationEventTypes.PrescriptionIssued, patientId, NotificationSubjects.Prescription, prescriptionId);
        await db.SaveChangesAsync();

        var second = await dispatcher.EnqueueAsync(NotificationEventTypes.PrescriptionIssued, patientId, NotificationSubjects.Prescription, prescriptionId);
        await db.SaveChangesAsync();

        Assert.Equal(first.Id, second.Id);
        Assert.Equal(1, await db.NotificationEvents.CountAsync());
    }

    [Fact]
    public async Task Dispatcher_Dispatch_SuccessFlow_MarksAcceptedByProvider()
    {
        using var db = CreateDb();
        var patient = new PatientProfile { FullName = "Bob Smith", Email = "bob@patientmail.com" };
        var doctor = new Doctor { FullName = "Dr. Adams", IsActive = true };
        var triage = new TriageRecord { Patient = patient, Symptoms = "Fever and sore throat for 3 days." };
        var rx = new Prescription
        {
            Patient = patient,
            TriageRecord = triage,
            IssuedByDoctorId = doctor.Id,
            Status = "Issued"
        };
        db.AddRange(patient, doctor, triage, rx);
        await db.SaveChangesAsync();

        var mockProvider = new MockEmailProvider
        {
            ResultToReturn = new EmailSendResult(EmailSendOutcome.Accepted, "resend_msg_001")
        };
        var opts = new NotificationOptions
        {
            Email = new EmailOptions { Provider = "Resend", ApiKey = "re_valid", From = "CareFlow <care@hospital.org>" }
        };
        var monitor = new SimpleOptionsMonitor<NotificationOptions>(opts);
        var resolver = new NotificationRecipientResolver(db);
        var dispatcher = new NotificationDispatcher(db, mockProvider, resolver, monitor, NullLogger<NotificationDispatcher>.Instance);

        var ev = await dispatcher.EnqueueAsync(NotificationEventTypes.PrescriptionIssued, patient.Id, NotificationSubjects.Prescription, rx.Id);
        await db.SaveChangesAsync();

        var dispatched = await dispatcher.DispatchAsync(ev.Id);

        Assert.NotNull(dispatched);
        Assert.Equal(NotificationStates.AcceptedByProvider, dispatched!.Status);
        Assert.Equal("resend_msg_001", dispatched.ProviderMessageId);
        Assert.Equal(1, mockProvider.SendCount);

        // Legacy mirror fields updated on prescription
        var updatedRx = await db.Prescriptions.FindAsync(rx.Id);
        Assert.True(updatedRx!.NotificationSent);
        Assert.Equal(NotificationChannels.Email, updatedRx.NotificationChannel);
        Assert.Equal(1, updatedRx.NotificationRetryCount);
    }

    [Fact]
    public async Task Dispatcher_Dispatch_WhenAlreadyAccepted_DoesNotResend()
    {
        using var db = CreateDb();
        var patient = new PatientProfile { FullName = "Bob Smith", Email = "bob@patientmail.com" };
        var doctor = new Doctor { FullName = "Dr. Adams", IsActive = true };
        var triage = new TriageRecord { Patient = patient, Symptoms = "Fever and sore throat for 3 days." };
        var rx = new Prescription { Patient = patient, TriageRecord = triage, Status = "Issued" };
        db.AddRange(patient, doctor, triage, rx);
        await db.SaveChangesAsync();

        var mockProvider = new MockEmailProvider();
        var opts = new NotificationOptions { Email = new EmailOptions { ApiKey = "re_valid", From = "CareFlow <c@h.org>" } };
        var monitor = new SimpleOptionsMonitor<NotificationOptions>(opts);
        var resolver = new NotificationRecipientResolver(db);
        var dispatcher = new NotificationDispatcher(db, mockProvider, resolver, monitor, NullLogger<NotificationDispatcher>.Instance);

        var ev = await dispatcher.EnqueueAsync(NotificationEventTypes.PrescriptionIssued, patient.Id, NotificationSubjects.Prescription, rx.Id);
        await db.SaveChangesAsync();

        // First dispatch
        await dispatcher.DispatchAsync(ev.Id);
        Assert.Equal(1, mockProvider.SendCount);

        // Second dispatch call
        await dispatcher.DispatchAsync(ev.Id);
        // Provider must NOT be called a second time
        Assert.Equal(1, mockProvider.SendCount);
    }

    [Fact]
    public async Task Dispatcher_AppointmentUnconfirmed_ReturnsNoLongerApplicable()
    {
        using var db = CreateDb();
        var patient = new PatientProfile { FullName = "Mark Lee", Email = "mark@patientmail.com" };
        var doctor = new Doctor { FullName = "Dr. Who", IsActive = true };
        var appt = new Appointment
        {
            Patient = patient,
            Doctor = doctor,
            AppointmentDate = new DateOnly(2026, 12, 1),
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(10, 30),
            Status = "Pending", // Not confirmed!
            ApprovalStatus = "Pending"
        };
        db.AddRange(patient, doctor, appt);
        await db.SaveChangesAsync();

        var mockProvider = new MockEmailProvider();
        var opts = new NotificationOptions { Email = new EmailOptions { ApiKey = "re_valid", From = "CareFlow <c@h.org>" } };
        var monitor = new SimpleOptionsMonitor<NotificationOptions>(opts);
        var resolver = new NotificationRecipientResolver(db);
        var dispatcher = new NotificationDispatcher(db, mockProvider, resolver, monitor, NullLogger<NotificationDispatcher>.Instance);

        var ev = await dispatcher.EnqueueAsync(NotificationEventTypes.AppointmentConfirmed, patient.Id, NotificationSubjects.Appointment, appt.Id);
        await db.SaveChangesAsync();

        var result = await dispatcher.DispatchAsync(ev.Id);

        Assert.Equal(NotificationStates.Failed, result!.Status);
        Assert.Equal(NotificationFailureCodes.NoLongerApplicable, result.FailureCode);
        Assert.Equal(0, mockProvider.SendCount);
    }

    [Fact]
    public async Task Dispatcher_AppointmentConfirmed_DispatchesSuccessfully()
    {
        using var db = CreateDb();
        var patient = new PatientProfile { FullName = "Mark Lee", Email = "mark@patientmail.com" };
        var doctor = new Doctor { FullName = "Dr. Who", IsActive = true };
        var appt = new Appointment
        {
            Patient = patient,
            Doctor = doctor,
            AppointmentDate = new DateOnly(2026, 12, 1),
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(10, 30),
            Status = "Confirmed",
            ApprovalStatus = "Approved"
        };
        db.AddRange(patient, doctor, appt);
        await db.SaveChangesAsync();

        var mockProvider = new MockEmailProvider();
        var opts = new NotificationOptions { Email = new EmailOptions { ApiKey = "re_valid", From = "CareFlow <c@h.org>" } };
        var monitor = new SimpleOptionsMonitor<NotificationOptions>(opts);
        var resolver = new NotificationRecipientResolver(db);
        var dispatcher = new NotificationDispatcher(db, mockProvider, resolver, monitor, NullLogger<NotificationDispatcher>.Instance);

        var ev = await dispatcher.EnqueueAsync(NotificationEventTypes.AppointmentConfirmed, patient.Id, NotificationSubjects.Appointment, appt.Id);
        await db.SaveChangesAsync();

        var result = await dispatcher.DispatchAsync(ev.Id);

        Assert.Equal(NotificationStates.AcceptedByProvider, result!.Status);
        Assert.Equal(1, mockProvider.SendCount);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 5. Webhooks & Svix Signature Verification Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void SvixSignatureVerifier_ValidSignature_Passes()
    {
        var rawKey = new byte[32];
        RandomNumberGenerator.Fill(rawKey);
        var secret = "whsec_" + Convert.ToBase64String(rawKey);

        var msgId = "msg_svix_test_001";
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var payload = "{\"type\":\"email.delivered\",\"data\":{\"email_id\":\"msg_123\"}}";

        var toSign = $"{msgId}.{timestamp}.{payload}";
        using var hmac = new HMACSHA256(rawKey);
        var sig = "v1," + Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(toSign)));

        var valid = SvixSignatureVerifier.Verify(
            secret, msgId, timestamp, sig, payload, DateTimeOffset.UtcNow, TimeSpan.FromMinutes(5));

        Assert.True(valid);
    }

    [Fact]
    public void SvixSignatureVerifier_TamperedBody_Fails()
    {
        var rawKey = new byte[32];
        RandomNumberGenerator.Fill(rawKey);
        var secret = "whsec_" + Convert.ToBase64String(rawKey);

        var msgId = "msg_svix_test_002";
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var originalPayload = "{\"type\":\"email.delivered\"}";
        var tamperedPayload = "{\"type\":\"email.bounced\"}";

        var toSign = $"{msgId}.{timestamp}.{originalPayload}";
        using var hmac = new HMACSHA256(rawKey);
        var sig = "v1," + Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(toSign)));

        var valid = SvixSignatureVerifier.Verify(
            secret, msgId, timestamp, sig, tamperedPayload, DateTimeOffset.UtcNow, TimeSpan.FromMinutes(5));

        Assert.False(valid);
    }

    [Fact]
    public async Task Dispatcher_ApplyProviderEvent_UpdatesStatusToDelivered_AndSuppressesDuplicates()
    {
        using var db = CreateDb();
        var ev = new NotificationEvent
        {
            EventType = NotificationEventTypes.PrescriptionIssued,
            SubjectType = NotificationSubjects.Prescription,
            SubjectId = Guid.NewGuid(),
            ProviderMessageId = "email_msg_svix_target",
            Status = NotificationStates.AcceptedByProvider
        };
        db.NotificationEvents.Add(ev);
        await db.SaveChangesAsync();

        var mockProvider = new MockEmailProvider();
        var opts = new NotificationOptions();
        var monitor = new SimpleOptionsMonitor<NotificationOptions>(opts);
        var resolver = new NotificationRecipientResolver(db);
        var dispatcher = new NotificationDispatcher(db, mockProvider, resolver, monitor, NullLogger<NotificationDispatcher>.Instance);

        // 1. First event
        var firstResult = await dispatcher.ApplyProviderEventAsync("email_msg_svix_target", "evt_101", "email.delivered");
        Assert.Equal(ProviderEventResult.Applied, firstResult);

        var updated = await db.NotificationEvents.FindAsync(ev.Id);
        Assert.Equal(NotificationStates.Delivered, updated!.Status);
        Assert.NotNull(updated.DeliveredAt);

        // 2. Duplicate event with same event id
        var dupResult = await dispatcher.ApplyProviderEventAsync("email_msg_svix_target", "evt_101", "email.delivered");
        Assert.Equal(ProviderEventResult.Duplicate, dupResult);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 6. NotificationsController Authorization Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task NotificationsController_DoctorCanOnlyRetryOwnPrescription()
    {
        using var db = CreateDb();
        var issuingDoctorId = Guid.NewGuid();
        var otherDoctorId = Guid.NewGuid();

        var patient = new PatientProfile { FullName = "Test Patient", Email = "patient@patientmail.com" };
        var rx = new Prescription
        {
            Patient = patient,
            IssuedByDoctorId = issuingDoctorId,
            Status = "Issued"
        };
        db.AddRange(patient, rx);

        var ev = new NotificationEvent
        {
            EventType = NotificationEventTypes.PrescriptionIssued,
            SubjectType = NotificationSubjects.Prescription,
            SubjectId = rx.Id,
            PatientProfileId = patient.Id,
            Status = NotificationStates.Failed,
            FailureCode = NotificationFailureCodes.RateLimited
        };
        db.NotificationEvents.Add(ev);
        await db.SaveChangesAsync();

        var mockProvider = new MockEmailProvider();
        var opts = new NotificationOptions();
        var monitor = new SimpleOptionsMonitor<NotificationOptions>(opts);
        var resolver = new NotificationRecipientResolver(db);
        var dispatcher = new NotificationDispatcher(db, mockProvider, resolver, monitor, NullLogger<NotificationDispatcher>.Instance);

        var controller = new NotificationsController(db, dispatcher, monitor);

        // Calling as other doctor
        var otherClaims = new[]
        {
            new Claim(ClaimTypes.Role, "Doctor"),
            new Claim("doctor_id", otherDoctorId.ToString())
        };
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(otherClaims, "Test"))
            }
        };

        var forbidResult = await controller.Retry(ev.Id);
        var objResult = Assert.IsType<ObjectResult>(forbidResult);
        Assert.Equal(403, objResult.StatusCode);

        // Calling as issuing doctor
        var issuingClaims = new[]
        {
            new Claim(ClaimTypes.Role, "Doctor"),
            new Claim("doctor_id", issuingDoctorId.ToString())
        };
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(issuingClaims, "Test"))
            }
        };

        var okResult = await controller.Retry(ev.Id);
        Assert.IsType<OkObjectResult>(okResult);
    }
}
