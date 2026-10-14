using System;
using System.Threading;
using System.Threading.Tasks;

namespace CareFlowAI.API.Services.Notifications
{
    public sealed record EmailMessage(string To, string Subject, string Html, string Text, string IdempotencyKey);

    public enum EmailSendOutcome
    {
        /// <summary>Provider returned success with a message id. Not proof of delivery.</summary>
        Accepted,
        NotConfigured,
        /// <summary>Permanent rejection (validation, unverified domain, test-mode restriction...).</summary>
        Rejected,
        AuthFailed,
        RateLimited,
        /// <summary>5xx / concurrent idempotent request / malformed success response.</summary>
        TemporaryFailure,
        Timeout,
        NetworkError
    }

    public sealed record EmailSendResult(
        EmailSendOutcome Outcome,
        string? ProviderMessageId = null,
        string? FailureReason = null,
        TimeSpan? RetryAfter = null)
    {
        public bool IsAccepted => Outcome == EmailSendOutcome.Accepted;
    }

    public interface IEmailProvider
    {
        /// <summary>True when an API key and sender are configured. Never throws.</summary>
        bool IsConfigured { get; }

        /// <summary>
        /// Sends one email. Never throws for provider/network problems: they are returned as an outcome with a
        /// sanitized reason (no provider response bodies, no secrets, no recipient address).
        /// </summary>
        Task<EmailSendResult> SendAsync(EmailMessage message, CancellationToken cancellationToken);
    }
}
