using System;

namespace CareFlowAI.API.Models
{
    /// <summary>
    /// Durable outbox row for one outbound notification (one row per event type + subject + channel).
    /// Created in the same database transaction as the business action it announces,
    /// and dispatched only after that transaction has committed.
    /// The row Id is the stable event identifier and is used as the provider idempotency key.
    /// No message content, medical details or full recipient address is stored.
    /// </summary>
    public class NotificationEvent
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>"PrescriptionIssued" | "AppointmentConfirmed"</summary>
        public string EventType { get; set; } = string.Empty;

        /// <summary>"Email" (SMS is not implemented).</summary>
        public string Channel { get; set; } = NotificationChannels.Email;

        /// <summary>Patient profile the notification is for. The recipient address is resolved at send time.</summary>
        public Guid PatientProfileId { get; set; }

        /// <summary>"Prescription" | "Appointment"</summary>
        public string SubjectType { get; set; } = string.Empty;
        public Guid SubjectId { get; set; }

        /// <summary>See <see cref="NotificationStates"/>.</summary>
        public string Status { get; set; } = NotificationStates.Queued;

        /// <summary>Masked recipient for display only (e.g. "s***@gmail.com").</summary>
        public string? RecipientMasked { get; set; }

        public int AttemptCount { get; set; }
        public DateTime? LastAttemptAt { get; set; }
        /// <summary>When the worker may automatically try again. Null means no automatic retry is scheduled.</summary>
        public DateTime? NextAttemptAt { get; set; }
        /// <summary>Claim lease: while in the future and Status=Sending, no other worker may dispatch this event.</summary>
        public DateTime? LockedUntil { get; set; }

        /// <summary>Provider message id (e.g. Resend email id). Proves provider acceptance only.</summary>
        public string? ProviderMessageId { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }

        /// <summary>
        /// Part of the provider idempotency key. Incremented only after a definite provider refusal (nothing was sent)
        /// so a corrected configuration is not blocked by a cached refusal. Ambiguous failures (timeout, 5xx) reuse the
        /// same key, which is what prevents duplicate emails.
        /// </summary>
        public int IdempotencyGeneration { get; set; }

        /// <summary>Last processed provider webhook event id (duplicate suppression).</summary>
        public string? LastProviderEventId { get; set; }

        /// <summary>Stable short code, see <see cref="NotificationFailureCodes"/>.</summary>
        public string? FailureCode { get; set; }
        /// <summary>Sanitized, human-readable failure reason. Never contains provider response bodies or secrets.</summary>
        public string? FailureReason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Optimistic concurrency token. Regenerated on every update so two dispatchers cannot both claim the event.</summary>
        public Guid ConcurrencyToken { get; set; } = Guid.NewGuid();
    }

    public static class NotificationChannels
    {
        public const string Email = "Email";
    }

    public static class NotificationEventTypes
    {
        public const string PrescriptionIssued = "PrescriptionIssued";
        public const string AppointmentConfirmed = "AppointmentConfirmed";
    }

    public static class NotificationSubjects
    {
        public const string Prescription = "Prescription";
        public const string Appointment = "Appointment";
    }

    public static class NotificationStates
    {
        public const string Queued = "Queued";
        public const string Sending = "Sending";
        /// <summary>The provider returned success and a message id. NOT proof of inbox delivery.</summary>
        public const string AcceptedByProvider = "AcceptedByProvider";
        /// <summary>The provider's delivery webhook confirmed delivery to the recipient's mail server.</summary>
        public const string Delivered = "Delivered";
        /// <summary>Provider accepted the message but later reported bounce/complaint/failure/suppression.</summary>
        public const string Bounced = "Bounced";
        /// <summary>Dispatch failed (see FailureCode); may be retried automatically or manually.</summary>
        public const string Failed = "Failed";
    }

    public static class NotificationFailureCodes
    {
        public const string ChannelDisabled = "ChannelDisabled";
        public const string ProviderNotConfigured = "ProviderNotConfigured";
        public const string NoRecipient = "NoRecipient";
        public const string InvalidRecipient = "InvalidRecipient";
        public const string NoLongerApplicable = "NoLongerApplicable";
        public const string ProviderRejected = "ProviderRejected";
        public const string ProviderAuth = "ProviderAuth";
        public const string RateLimited = "RateLimited";
        public const string ProviderTemporary = "ProviderTemporary";
        public const string Timeout = "Timeout";
        public const string NetworkError = "NetworkError";
        public const string ProviderBounced = "ProviderBounced";

        /// <summary>Failures worth retrying automatically (transient). Everything else needs a human/config fix.</summary>
        public static bool IsTransient(string? code) =>
            code is RateLimited or ProviderTemporary or Timeout or NetworkError;
    }
}
