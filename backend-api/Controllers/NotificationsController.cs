using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CareFlowAI.API.Data;
using CareFlowAI.API.DTOs;
using CareFlowAI.API.Models;
using CareFlowAI.API.Services.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CareFlowAI.API.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    public class NotificationsController : ControllerBase
    {
        private const int MaxWebhookBytes = 64 * 1024;
        private static readonly TimeSpan WebhookTolerance = TimeSpan.FromMinutes(5);

        private readonly ApplicationDbContext _db;
        private readonly INotificationDispatcher _dispatcher;
        private readonly IOptionsMonitor<NotificationOptions> _options;

        public NotificationsController(
            ApplicationDbContext db,
            INotificationDispatcher dispatcher,
            IOptionsMonitor<NotificationOptions> options)
        {
            _db = db;
            _dispatcher = dispatcher;
            _options = options;
        }

        // ── GET api/notifications/status ─────────────────────────────────────
        /// <summary>Honest configuration health for staff. Never returns secrets.</summary>
        [Authorize(Roles = "Doctor,Staff,Admin")]
        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            var o = _options.CurrentValue;
            return Ok(new
            {
                emailEnabled = o.IsChannelEnabled(NotificationChannels.Email),
                emailConfigured = o.Email.IsConfigured,
                provider = o.Email.Provider,
                deliveryTrackingConfigured = !string.IsNullOrWhiteSpace(o.Email.WebhookSecret),
                workerEnabled = o.WorkerEnabled,
                smsImplemented = false
            });
        }

        // ── POST api/notifications/{id}/retry ────────────────────────────────
        /// <summary>
        /// Manual retry of a failed notification. Staff/Admin may retry any; a Doctor only notifications for
        /// prescriptions they issued or appointments assigned to them. Accepted/delivered emails are never re-sent.
        /// </summary>
        [Authorize(Roles = "Doctor,Staff,Admin")]
        [HttpPost("{id:guid}/retry")]
        public async Task<IActionResult> Retry(Guid id)
        {
            var ev = await _db.NotificationEvents.FirstOrDefaultAsync(e => e.Id == id);
            if (ev == null) return NotFound(new { message = "Notification not found." });

            if (!await NotificationAccess.CanRetryAsync(User, _db, ev))
                return StatusCode(403, new { message = "You are not permitted to retry this notification." });

            return await NotificationAccess.RetryAsync(this, _dispatcher, ev);
        }

        // ── POST api/notifications/webhooks/resend ───────────────────────────
        /// <summary>
        /// Resend delivery webhook. Anonymous by necessity, protected by a verified Svix signature and a
        /// 5-minute timestamp window. Duplicate and out-of-order events are harmless (see dispatcher).
        /// </summary>
        [AllowAnonymous]
        [HttpPost("webhooks/resend")]
        public async Task<IActionResult> ResendWebhook()
        {
            var secret = _options.CurrentValue.Email.WebhookSecret;
            if (string.IsNullOrWhiteSpace(secret))
                return StatusCode(503, new { message = "Delivery webhooks are not configured." });

            if (Request.ContentLength is > MaxWebhookBytes)
                return StatusCode(413);

            Request.EnableBuffering();
            string body;
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true))
                body = await reader.ReadToEndAsync();
            if (body.Length > MaxWebhookBytes) return StatusCode(413);

            var svixId = Request.Headers["svix-id"].ToString();
            var valid = SvixSignatureVerifier.Verify(
                secret,
                svixId,
                Request.Headers["svix-timestamp"].ToString(),
                Request.Headers["svix-signature"].ToString(),
                body,
                DateTimeOffset.UtcNow,
                WebhookTolerance);
            if (!valid) return Unauthorized(new { message = "Invalid webhook signature." });

            string? type, emailId;
            try
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;
                type = root.TryGetProperty("type", out var t) ? t.GetString() : null;
                emailId = root.TryGetProperty("data", out var d) && d.ValueKind == JsonValueKind.Object
                          && d.TryGetProperty("email_id", out var e) ? e.GetString() : null;
            }
            catch (JsonException)
            {
                return BadRequest(new { message = "Malformed payload." });
            }

            if (string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(emailId))
                return Ok(new { result = "ignored" });

            var result = await _dispatcher.ApplyProviderEventAsync(emailId, svixId, type);
            return Ok(new { result = result.ToString() });
        }
    }

    /// <summary>Shared retry authorization and behaviour (used by the notifications and prescriptions controllers).</summary>
    public static class NotificationAccess
    {
        public static async Task<bool> CanRetryAsync(ClaimsPrincipal user, ApplicationDbContext db, NotificationEvent ev)
        {
            if (user.IsInRole("Admin") || user.IsInRole("Staff")) return true;
            if (!user.IsInRole("Doctor")) return false;
            if (!Guid.TryParse(user.FindFirst("doctor_id")?.Value, out var doctorId)) return false;

            return ev.SubjectType switch
            {
                NotificationSubjects.Prescription => await db.Prescriptions.AnyAsync(p =>
                    p.Id == ev.SubjectId && p.IssuedByDoctorId == doctorId),
                NotificationSubjects.Appointment => await db.Appointments.AnyAsync(a =>
                    a.Id == ev.SubjectId && a.DoctorId == doctorId),
                _ => false
            };
        }

        public static async Task<IActionResult> RetryAsync(ControllerBase controller, INotificationDispatcher dispatcher, NotificationEvent ev)
        {
            switch (ev.Status)
            {
                case NotificationStates.AcceptedByProvider:
                case NotificationStates.Delivered:
                    return controller.Conflict(new
                    {
                        message = "This email was already handed to the provider. It will not be sent again.",
                        notification = NotificationStatusDto.From(ev)
                    });
                case NotificationStates.Bounced:
                    return controller.Conflict(new
                    {
                        message = "The provider reported this email as undeliverable. Correct the patient's email address first.",
                        notification = NotificationStatusDto.From(ev)
                    });
                case NotificationStates.Sending when ev.LockedUntil > DateTime.UtcNow:
                    return controller.Conflict(new
                    {
                        message = "This notification is already being sent.",
                        notification = NotificationStatusDto.From(ev)
                    });
            }

            var after = await dispatcher.DispatchAsync(ev.Id, manual: true);
            var dto = NotificationStatusDto.From(after);
            return controller.Ok(new
            {
                message = Describe(dto),
                notification = dto
            });
        }

        public static string Describe(NotificationStatusDto dto) => dto.State switch
        {
            NotificationStates.AcceptedByProvider => "Email accepted by the provider. Delivery is not yet confirmed.",
            NotificationStates.Delivered => "Email delivered.",
            NotificationStates.Queued => "Email queued for sending.",
            NotificationStates.Sending => "Email is being sent.",
            NotificationStates.Bounced => $"Email not delivered: {dto.Detail}",
            NotificationStates.Failed => $"Email failed: {dto.Detail}" +
                                         (dto.WillRetryAutomatically ? " It will be retried automatically." : string.Empty),
            _ => dto.Label
        };
    }
}
