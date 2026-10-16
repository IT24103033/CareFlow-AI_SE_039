using System;
using System.Linq;

namespace CareFlowAI.API.Services.Notifications
{
    /// <summary>
    /// Bound from the "Notifications" configuration section. On Render set these as environment variables
    /// using the double-underscore form, e.g. Notifications__Email__ApiKey. Secrets must never be committed.
    /// </summary>
    public class NotificationOptions
    {
        public const string Section = "Notifications";

        /// <summary>Comma-separated list of enabled channels. Only "Email" is implemented.</summary>
        public string EnabledChannels { get; set; } = "Email";

        /// <summary>Run the background outbox worker that retries transient failures.</summary>
        public bool WorkerEnabled { get; set; } = true;

        /// <summary>Public web/app URL shown in notification emails ("open CareFlow"). Optional.</summary>
        public string? AppUrl { get; set; }

        /// <summary>Upper bound for the inline (in-request) dispatch attempt.</summary>
        public int InlineDispatchTimeoutSeconds { get; set; } = 12;

        /// <summary>Maximum automatic attempts per event. Manual retries are still allowed afterwards.</summary>
        public int MaxAutoAttempts { get; set; } = 4;

        public EmailOptions Email { get; set; } = new();

        public bool IsChannelEnabled(string channel) =>
            (EnabledChannels ?? string.Empty)
                .Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Any(c => string.Equals(c, channel, StringComparison.OrdinalIgnoreCase));
    }

    public class EmailOptions
    {
        /// <summary>Only "Resend" is supported.</summary>
        public string Provider { get; set; } = "Resend";

        /// <summary>Provider API key (secret). Environment variable only.</summary>
        public string? ApiKey { get; set; }

        /// <summary>Sender, e.g. "CareFlow AI &lt;notifications@your-verified-domain.com&gt;". Must use a verified domain.</summary>
        public string? From { get; set; }

        public string ApiBaseUrl { get; set; } = "https://api.resend.com";

        public int TimeoutSeconds { get; set; } = 10;

        /// <summary>Resend/Svix webhook signing secret ("whsec_..."). Optional; enables delivery confirmation.</summary>
        public string? WebhookSecret { get; set; }

        public bool IsConfigured =>
            string.Equals(Provider, "Resend", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(ApiKey)
            && !string.IsNullOrWhiteSpace(From)
            && Uri.TryCreate(ApiBaseUrl, UriKind.Absolute, out var u) && u.Scheme == Uri.UriSchemeHttps;
    }
}
