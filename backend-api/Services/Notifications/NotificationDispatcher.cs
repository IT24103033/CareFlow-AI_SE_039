using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CareFlowAI.API.Data;
using CareFlowAI.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CareFlowAI.API.Services.Notifications
{
    public enum ProviderEventResult { Applied, Duplicate, Ignored, UnknownMessage }

    public interface INotificationDispatcher
    {
        /// <summary>
        /// Stages a Queued outbox row on the current DbContext (does NOT save). Call it before the business
        /// SaveChanges so the row is committed atomically with the business action. Returns the existing row
        /// when one already exists for (eventType, subject, channel), so duplicates are impossible.
        /// </summary>
        Task<NotificationEvent> EnqueueAsync(
            string eventType, Guid patientProfileId, string subjectType, Guid subjectId, CancellationToken ct = default);

        /// <summary>
        /// Attempts to send the event. Safe to call concurrently and repeatedly: only one caller can claim an event,
        /// and events already accepted/delivered are never re-sent. <paramref name="manual"/> bypasses the
        /// automatic retry schedule (used by the permitted retry action).
        /// </summary>
        Task<NotificationEvent?> DispatchAsync(Guid eventId, bool manual = false, CancellationToken ct = default);

        /// <summary>Bounded, non-throwing dispatch used right after the business transaction commits.</summary>
        Task<NotificationEvent?> TryDispatchInlineAsync(Guid eventId, CancellationToken ct = default);

        /// <summary>Applies a verified provider delivery event. Duplicate and out-of-order events are harmless.</summary>
        Task<ProviderEventResult> ApplyProviderEventAsync(
            string providerMessageId, string providerEventId, string providerEventType, CancellationToken ct = default);
    }

    public class NotificationDispatcher : INotificationDispatcher
    {
        private static readonly TimeSpan ClaimLease = TimeSpan.FromSeconds(90);
        private static readonly TimeSpan[] Backoff =
        {
            TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(30)
        };

        private readonly ApplicationDbContext _db;
        private readonly IEmailProvider _email;
        private readonly NotificationRecipientResolver _resolver;
        private readonly IOptionsMonitor<NotificationOptions> _options;
        private readonly ILogger<NotificationDispatcher> _logger;

        public NotificationDispatcher(
            ApplicationDbContext db,
            IEmailProvider email,
            NotificationRecipientResolver resolver,
            IOptionsMonitor<NotificationOptions> options,
            ILogger<NotificationDispatcher> logger)
        {
            _db = db;
            _email = email;
            _resolver = resolver;
            _options = options;
            _logger = logger;
        }

        private NotificationOptions Options => _options.CurrentValue;

        // ── Enqueue ──────────────────────────────────────────────────────────

        public async Task<NotificationEvent> EnqueueAsync(
            string eventType, Guid patientProfileId, string subjectType, Guid subjectId, CancellationToken ct = default)
        {
            const string channel = NotificationChannels.Email;

            var existing = _db.NotificationEvents.Local.FirstOrDefault(e =>
                    e.EventType == eventType && e.SubjectId == subjectId && e.Channel == channel)
                ?? await _db.NotificationEvents.FirstOrDefaultAsync(e =>
                    e.EventType == eventType && e.SubjectId == subjectId && e.Channel == channel, ct);
            if (existing != null) return existing;

            var ev = new NotificationEvent
            {
                EventType = eventType,
                Channel = channel,
                PatientProfileId = patientProfileId,
                SubjectType = subjectType,
                SubjectId = subjectId,
                Status = NotificationStates.Queued,
                NextAttemptAt = DateTime.UtcNow
            };
            _db.NotificationEvents.Add(ev);
            return ev;
        }

        // ── Dispatch ─────────────────────────────────────────────────────────

        public async Task<NotificationEvent?> TryDispatchInlineAsync(Guid eventId, CancellationToken ct = default)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(Math.Clamp(Options.InlineDispatchTimeoutSeconds, 2, 60)));
            try
            {
                return await DispatchAsync(eventId, manual: false, cts.Token);
            }
            catch (Exception ex) when (ex is not OutOfMemoryException)
            {
                // The business action is already committed: a notification problem must never surface as a failure of it.
                _logger.LogError("Inline notification dispatch for event {EventId} failed: {ErrorType}", eventId, ex.GetType().Name);
                return await _db.NotificationEvents.AsNoTracking().FirstOrDefaultAsync(e => e.Id == eventId, CancellationToken.None);
            }
        }

        public async Task<NotificationEvent?> DispatchAsync(Guid eventId, bool manual = false, CancellationToken ct = default)
        {
            var ev = await _db.NotificationEvents.FirstOrDefaultAsync(e => e.Id == eventId, ct);
            if (ev == null) return null;
            await _db.Entry(ev).ReloadAsync(ct); // never act on a stale tracked copy

            var now = DateTime.UtcNow;

            // Never re-send something the provider already accepted (or that bounced).
            if (ev.Status is NotificationStates.AcceptedByProvider or NotificationStates.Delivered or NotificationStates.Bounced)
                return ev;

            // Someone else is sending right now.
            if (ev.Status == NotificationStates.Sending && ev.LockedUntil > now)
                return ev;

            if (!manual && !IsDueForAutomaticAttempt(ev, now))
                return ev;

            // ── Claim (optimistic concurrency: exactly one caller wins) ──────
            ev.Status = NotificationStates.Sending;
            ev.LockedUntil = now + ClaimLease;
            ev.AttemptCount += 1;
            ev.LastAttemptAt = now;
            ev.NextAttemptAt = null;
            ev.UpdatedAt = now;
            ev.ConcurrencyToken = Guid.NewGuid();
            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateConcurrencyException)
            {
                await _db.Entry(ev).ReloadAsync(ct);
                return ev;
            }

            // ── Send (outside any business transaction) ──────────────────────
            var attempt = await BuildAndSendAsync(ev, ct);

            // ── Record the outcome ───────────────────────────────────────────
            for (var i = 0; i < 3; i++)
            {
                ApplyAttemptOutcome(ev, attempt, DateTime.UtcNow);
                await MirrorToPrescriptionAsync(ev, ct);
                try
                {
                    await _db.SaveChangesAsync(ct);
                    return ev;
                }
                catch (DbUpdateConcurrencyException)
                {
                    await _db.Entry(ev).ReloadAsync(ct);
                    if (ev.Status is NotificationStates.AcceptedByProvider or NotificationStates.Delivered or NotificationStates.Bounced)
                        return ev;
                }
            }
            return ev;
        }

        private bool IsDueForAutomaticAttempt(NotificationEvent ev, DateTime now) => ev.Status switch
        {
            NotificationStates.Queued => true,
            NotificationStates.Sending => true, // lease already expired (checked by caller)
            NotificationStates.Failed => ev.NextAttemptAt != null
                                         && ev.NextAttemptAt <= now
                                         && NotificationFailureCodes.IsTransient(ev.FailureCode)
                                         && ev.AttemptCount < Options.MaxAutoAttempts,
            _ => false
        };

        private sealed record Attempt(
            EmailSendResult? Send, string? FailureCode, string? FailureReason, string? RecipientMasked);

        private async Task<Attempt> BuildAndSendAsync(NotificationEvent ev, CancellationToken ct)
        {
            try
            {
                if (!Options.IsChannelEnabled(ev.Channel))
                    return new Attempt(null, NotificationFailureCodes.ChannelDisabled,
                        "Email notifications are disabled on the server.", null);

                if (!_email.IsConfigured)
                    return new Attempt(null, NotificationFailureCodes.ProviderNotConfigured,
                        "Email delivery is not configured on the server.", null);

                var content = await BuildContentAsync(ev, ct);
                if (content == null)
                    return new Attempt(null, NotificationFailureCodes.NoLongerApplicable,
                        "The prescription or appointment is no longer eligible for this notification.", null);

                var recipient = await _resolver.ResolveEmailAsync(ev.PatientProfileId, ct);
                if (!recipient.Ok)
                    return new Attempt(null, recipient.FailureCode, recipient.FailureReason, null);

                var masked = NotificationRecipientResolver.Mask(recipient.Email!);
                var message = new EmailMessage(
                    recipient.Email!, content.Subject, content.Html, content.Text,
                    IdempotencyKey: $"careflow-{ev.Id:N}-g{ev.IdempotencyGeneration}");

                var send = await _email.SendAsync(message, ct);
                return new Attempt(send, null, null, masked);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                return new Attempt(null, NotificationFailureCodes.Timeout,
                    "The notification could not be sent in time and will be retried.", null);
            }
        }

        private async Task<NotificationContentBuilder.Content?> BuildContentAsync(NotificationEvent ev, CancellationToken ct)
        {
            var patientName = await _db.PatientProfiles.AsNoTracking()
                .Where(p => p.Id == ev.PatientProfileId).Select(p => p.FullName).FirstOrDefaultAsync(ct);

            switch (ev.EventType)
            {
                case NotificationEventTypes.PrescriptionIssued:
                {
                    var rx = await _db.Prescriptions.AsNoTracking()
                        .Where(p => p.Id == ev.SubjectId)
                        .Select(p => new { p.Status, p.PatientId })
                        .FirstOrDefaultAsync(ct);
                    if (rx == null || rx.PatientId != ev.PatientProfileId
                        || (rx.Status != "Issued" && rx.Status != "Dispensed"))
                        return null;
                    return NotificationContentBuilder.PrescriptionIssued(patientName, Options.AppUrl);
                }
                case NotificationEventTypes.AppointmentConfirmed:
                {
                    var appt = await _db.Appointments.AsNoTracking()
                        .Where(a => a.Id == ev.SubjectId)
                        .Select(a => new
                        {
                            a.Status, a.ApprovalStatus, a.PatientId, a.AppointmentDate, a.StartTime,
                            DoctorName = a.Doctor.FullName
                        })
                        .FirstOrDefaultAsync(ct);
                    if (appt == null || appt.PatientId != ev.PatientProfileId
                        || appt.Status != "Confirmed" || appt.ApprovalStatus != "Approved")
                        return null;
                    return NotificationContentBuilder.AppointmentConfirmed(
                        patientName, appt.AppointmentDate, appt.StartTime, appt.DoctorName, Options.AppUrl);
                }
                default:
                    return null;
            }
        }

        private void ApplyAttemptOutcome(NotificationEvent ev, Attempt a, DateTime now)
        {
            ev.UpdatedAt = now;
            ev.LockedUntil = null;
            ev.ConcurrencyToken = Guid.NewGuid();
            if (a.RecipientMasked != null) ev.RecipientMasked = a.RecipientMasked;

            if (a.Send is { IsAccepted: true } ok)
            {
                ev.Status = NotificationStates.AcceptedByProvider;
                ev.ProviderMessageId = ok.ProviderMessageId;
                ev.AcceptedAt = now;
                ev.FailureCode = null;
                ev.FailureReason = null;
                ev.NextAttemptAt = null;
                return;
            }

            string code;
            string? reason;
            TimeSpan? retryAfter = null;
            if (a.Send != null)
            {
                code = a.Send.Outcome switch
                {
                    EmailSendOutcome.NotConfigured => NotificationFailureCodes.ProviderNotConfigured,
                    EmailSendOutcome.Rejected => NotificationFailureCodes.ProviderRejected,
                    EmailSendOutcome.AuthFailed => NotificationFailureCodes.ProviderAuth,
                    EmailSendOutcome.RateLimited => NotificationFailureCodes.RateLimited,
                    EmailSendOutcome.Timeout => NotificationFailureCodes.Timeout,
                    EmailSendOutcome.NetworkError => NotificationFailureCodes.NetworkError,
                    _ => NotificationFailureCodes.ProviderTemporary
                };
                reason = a.Send.FailureReason;
                retryAfter = a.Send.RetryAfter;
            }
            else
            {
                code = a.FailureCode ?? NotificationFailureCodes.ProviderTemporary;
                reason = a.FailureReason;
            }

            ev.Status = NotificationStates.Failed;
            ev.FailureCode = code;
            ev.FailureReason = Truncate(reason ?? "Notification could not be sent.", 300);

            // A definite provider refusal means nothing was sent: use a fresh idempotency key on the next try
            // so the provider does not replay the cached refusal after the cause (e.g. domain) has been fixed.
            if (code is NotificationFailureCodes.ProviderRejected or NotificationFailureCodes.ProviderAuth)
                ev.IdempotencyGeneration += 1;

            if (NotificationFailureCodes.IsTransient(code) && ev.AttemptCount < Options.MaxAutoAttempts)
            {
                var delay = retryAfter is { } ra
                    ? TimeSpan.FromSeconds(Math.Clamp(ra.TotalSeconds, 5, 900))
                    : Backoff[Math.Min(Math.Max(ev.AttemptCount - 1, 0), Backoff.Length - 1)];
                ev.NextAttemptAt = now + delay;
            }
            else
            {
                ev.NextAttemptAt = null;
            }
        }

        // ── Provider delivery events ─────────────────────────────────────────

        public async Task<ProviderEventResult> ApplyProviderEventAsync(
            string providerMessageId, string providerEventId, string providerEventType, CancellationToken ct = default)
        {
            var target = providerEventType switch
            {
                "email.delivered" => NotificationStates.Delivered,
                "email.bounced" or "email.failed" or "email.suppressed" => NotificationStates.Bounced,
                _ => null // sent / opened / clicked / delivery_delayed / complained: informational only
            };
            if (target == null) return ProviderEventResult.Ignored;

            for (var i = 0; i < 3; i++)
            {
                var ev = await _db.NotificationEvents.FirstOrDefaultAsync(e => e.ProviderMessageId == providerMessageId, ct);
                if (ev == null) return ProviderEventResult.UnknownMessage;
                await _db.Entry(ev).ReloadAsync(ct);

                if (ev.LastProviderEventId == providerEventId) return ProviderEventResult.Duplicate;

                // Only an accepted message can move to a final provider state. Delivered/Bounced never regress or flip.
                if (ev.Status != NotificationStates.AcceptedByProvider)
                    return ProviderEventResult.Ignored;

                var now = DateTime.UtcNow;
                ev.Status = target;
                ev.LastProviderEventId = providerEventId;
                ev.UpdatedAt = now;
                ev.ConcurrencyToken = Guid.NewGuid();
                if (target == NotificationStates.Delivered)
                {
                    ev.DeliveredAt = now;
                }
                else
                {
                    ev.FailureCode = NotificationFailureCodes.ProviderBounced;
                    ev.FailureReason = "The email provider reported that the message could not be delivered to the recipient.";
                }
                await MirrorToPrescriptionAsync(ev, ct);
                try
                {
                    await _db.SaveChangesAsync(ct);
                    return ProviderEventResult.Applied;
                }
                catch (DbUpdateConcurrencyException)
                {
                    // re-read and re-evaluate (duplicate / already final)
                }
            }
            return ProviderEventResult.Ignored;
        }

        // ── Legacy Prescription columns kept consistent with the real state ──

        private async Task MirrorToPrescriptionAsync(NotificationEvent ev, CancellationToken ct)
        {
            if (ev.SubjectType != NotificationSubjects.Prescription) return;
            var rx = await _db.Prescriptions.FirstOrDefaultAsync(p => p.Id == ev.SubjectId, ct);
            if (rx == null) return;

            var sent = ev.Status is NotificationStates.AcceptedByProvider or NotificationStates.Delivered;
            rx.NotificationSent = sent;
            rx.NotificationChannel = sent ? NotificationChannels.Email : null;
            rx.NotifiedAt = sent ? (ev.DeliveredAt ?? ev.AcceptedAt) : null;
            rx.NotificationFailureReason = sent ? null : ev.FailureReason;
            rx.NotificationRetryCount = ev.AttemptCount;
        }

        private static string Truncate(string s, int max) => s.Length <= max ? s : s[..max];
    }
}
