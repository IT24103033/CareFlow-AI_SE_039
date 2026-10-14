using System;
using CareFlowAI.API.Models;

namespace CareFlowAI.API.DTOs
{
    /// <summary>
    /// API/UI view of a notification. Derived solely from the persisted outbox state; never contains
    /// secrets, provider response bodies, or the full recipient address.
    /// </summary>
    public class NotificationStatusDto
    {
        public Guid? Id { get; set; }
        public string Channel { get; set; } = NotificationChannels.Email;

        /// <summary>NotApplicable | NotSent | Queued | Sending | AcceptedByProvider | Delivered | Bounced | Failed</summary>
        public string State { get; set; } = "NotSent";
        public string Label { get; set; } = string.Empty;
        public string? Detail { get; set; }

        public string? RecipientMasked { get; set; }
        public int Attempts { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? LastAttemptAt { get; set; }
        public DateTime? NextAttemptAt { get; set; }
        public string? FailureCode { get; set; }

        /// <summary>True when an automatic retry is already scheduled.</summary>
        public bool WillRetryAutomatically { get; set; }

        /// <summary>True when a permitted user may trigger a manual retry.</summary>
        public bool CanRetry { get; set; }

        public static NotificationStatusDto NotApplicable(string label) =>
            new() { State = "NotApplicable", Label = label };

        /// <param name="ev">The outbox row, or null when none exists.</param>
        /// <param name="legacyRetryableWithoutEvent">
        /// True for records created before email notifications existed (no row) that are still eligible,
        /// so an authorized user can send the first notification.
        /// </param>
        public static NotificationStatusDto From(NotificationEvent? ev, bool legacyRetryableWithoutEvent = false)
        {
            if (ev == null)
            {
                return new NotificationStatusDto
                {
                    State = "NotSent",
                    Label = "No email sent",
                    Detail = legacyRetryableWithoutEvent
                        ? "No email notification was recorded for this item."
                        : null,
                    CanRetry = legacyRetryableWithoutEvent
                };
            }

            var dto = new NotificationStatusDto
            {
                Id = ev.Id,
                Channel = ev.Channel,
                State = ev.Status,
                RecipientMasked = ev.RecipientMasked,
                Attempts = ev.AttemptCount,
                CreatedAt = ev.CreatedAt,
                AcceptedAt = ev.AcceptedAt,
                DeliveredAt = ev.DeliveredAt,
                LastAttemptAt = ev.LastAttemptAt,
                NextAttemptAt = ev.NextAttemptAt,
                FailureCode = ev.FailureCode
            };

            switch (ev.Status)
            {
                case NotificationStates.Queued:
                    dto.Label = "Email queued";
                    dto.Detail = "The email is waiting to be sent.";
                    break;
                case NotificationStates.Sending:
                    dto.Label = "Sending email";
                    break;
                case NotificationStates.AcceptedByProvider:
                    dto.Label = "Email accepted by provider";
                    dto.Detail = "Handed to the email provider. Delivery to the inbox is not confirmed.";
                    break;
                case NotificationStates.Delivered:
                    dto.Label = "Email delivered";
                    dto.Detail = "The email provider confirmed delivery to the recipient's mail server.";
                    break;
                case NotificationStates.Bounced:
                    dto.Label = "Email not delivered";
                    dto.Detail = ev.FailureReason;
                    break;
                default:
                    dto.Label = "Email failed";
                    dto.Detail = ev.FailureReason;
                    dto.WillRetryAutomatically = ev.NextAttemptAt != null
                        && NotificationFailureCodes.IsTransient(ev.FailureCode);
                    dto.CanRetry = ev.FailureCode != NotificationFailureCodes.NoLongerApplicable;
                    break;
            }
            return dto;
        }
    }
}
