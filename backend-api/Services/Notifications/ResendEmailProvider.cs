using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CareFlowAI.API.Services.Notifications
{
    /// <summary>
    /// Resend transactional email provider (POST https://api.resend.com/emails).
    /// Uses the Idempotency-Key header so retries of the same event can never send a second email within 24h.
    /// Failure reasons are mapped to fixed, sanitized messages: provider response bodies are never stored or returned.
    /// </summary>
    public class ResendEmailProvider : IEmailProvider
    {
        private readonly HttpClient _http;
        private readonly IOptionsMonitor<NotificationOptions> _options;
        private readonly ILogger<ResendEmailProvider> _logger;

        public ResendEmailProvider(
            HttpClient http,
            IOptionsMonitor<NotificationOptions> options,
            ILogger<ResendEmailProvider> logger)
        {
            _http = http;
            _options = options;
            _logger = logger;
        }

        public bool IsConfigured => _options.CurrentValue.Email.IsConfigured;

        public async Task<EmailSendResult> SendAsync(EmailMessage message, CancellationToken cancellationToken)
        {
            var email = _options.CurrentValue.Email;
            if (!email.IsConfigured)
                return new EmailSendResult(EmailSendOutcome.NotConfigured,
                    FailureReason: "Email delivery is not configured on the server.");

            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(Math.Clamp(email.TimeoutSeconds, 2, 60)));

            using var request = new HttpRequestMessage(
                HttpMethod.Post, email.ApiBaseUrl.TrimEnd('/') + "/emails")
            {
                Content = JsonContent.Create(new
                {
                    from = email.From,
                    to = new[] { message.To },
                    subject = message.Subject,
                    html = message.Html,
                    text = message.Text
                })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", email.ApiKey);
            // Resend: 1-256 chars, kept for 24h. Same key => same response, no second email.
            request.Headers.TryAddWithoutValidation("Idempotency-Key", message.IdempotencyKey);

            HttpResponseMessage response;
            try
            {
                response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Email provider request timed out.");
                return new EmailSendResult(EmailSendOutcome.Timeout,
                    FailureReason: "The email provider did not respond in time.");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning("Email provider request failed: {ErrorType}", ex.GetType().Name);
                return new EmailSendResult(EmailSendOutcome.NetworkError,
                    FailureReason: "Could not reach the email provider.");
            }

            using (response)
            {
                var status = (int)response.StatusCode;
                var errorName = await TryReadErrorNameAsync(response, timeout.Token);

                if (response.IsSuccessStatusCode)
                {
                    var id = await TryReadIdAsync(response, timeout.Token);
                    if (string.IsNullOrWhiteSpace(id))
                    {
                        _logger.LogWarning("Email provider returned {Status} without a message id.", status);
                        return new EmailSendResult(EmailSendOutcome.TemporaryFailure,
                            FailureReason: "The email provider returned an unexpected response.");
                    }
                    return new EmailSendResult(EmailSendOutcome.Accepted, ProviderMessageId: id);
                }

                _logger.LogWarning("Email provider rejected request: HTTP {Status} ({ErrorName}).", status, errorName ?? "unknown");

                return status switch
                {
                    (int)HttpStatusCode.TooManyRequests => new EmailSendResult(
                        EmailSendOutcome.RateLimited,
                        FailureReason: "The email provider is rate limiting requests. It will be retried.",
                        RetryAfter: ReadRetryAfter(response)),
                    (int)HttpStatusCode.Unauthorized => new EmailSendResult(
                        EmailSendOutcome.AuthFailed,
                        FailureReason: "The email provider rejected the server's API key."),
                    (int)HttpStatusCode.Forbidden => new EmailSendResult(
                        EmailSendOutcome.Rejected,
                        FailureReason: "The email provider refused the message: the sender domain is not verified, " +
                                       "the API key is not allowed to send, or the account is in test mode and can only email its owner."),
                    (int)HttpStatusCode.Conflict when errorName == "concurrent_idempotent_requests" => new EmailSendResult(
                        EmailSendOutcome.TemporaryFailure,
                        FailureReason: "Another send for this notification is in progress."),
                    >= 500 => new EmailSendResult(
                        EmailSendOutcome.TemporaryFailure,
                        FailureReason: "The email provider had a temporary error."),
                    _ => new EmailSendResult(
                        EmailSendOutcome.Rejected,
                        FailureReason: "The email provider rejected the message.")
                };
            }
        }

        private static TimeSpan? ReadRetryAfter(HttpResponseMessage response)
        {
            if (response.Headers.RetryAfter?.Delta is { } delta) return delta;
            if (response.Headers.TryGetValues("retry-after", out var values)
                && int.TryParse(System.Linq.Enumerable.FirstOrDefault(values), out var seconds) && seconds >= 0)
                return TimeSpan.FromSeconds(seconds);
            return null;
        }

        private static async Task<string?> TryReadIdAsync(HttpResponseMessage response, CancellationToken ct)
        {
            try
            {
                using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
                return doc.RootElement.TryGetProperty("id", out var id) && id.ValueKind == JsonValueKind.String
                    ? id.GetString()
                    : null;
            }
            catch (Exception ex) when (ex is JsonException or InvalidOperationException or HttpRequestException)
            {
                return null;
            }
        }

        /// <summary>Reads only the machine-readable error "name" (e.g. validation_error). The free-text message is ignored.</summary>
        private static async Task<string?> TryReadErrorNameAsync(HttpResponseMessage response, CancellationToken ct)
        {
            if (response.IsSuccessStatusCode) return null;
            try
            {
                using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
                return doc.RootElement.TryGetProperty("name", out var n) && n.ValueKind == JsonValueKind.String
                    ? n.GetString()
                    : null;
            }
            catch (Exception ex) when (ex is JsonException or InvalidOperationException or HttpRequestException)
            {
                return null;
            }
        }
    }
}
