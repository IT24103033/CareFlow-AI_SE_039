using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CareFlowAI.API.Services
{
    /// <summary>
    /// Component D – Third-Party Notification Service (SMS &amp; Email)
    /// Dispatches notifications via external HTTP providers when e-prescriptions are issued or approved.
    /// No hardcoded recipient addresses. Contact details must come from the patient record.
    /// </summary>
    public interface INotificationService
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string messageBody);
        Task<bool> SendSmsAsync(string phoneNumber, string message);

        /// <summary>
        /// Dispatches a prescription-issued notification to the actual patient.
        /// </summary>
        /// <param name="patientName">Patient's full name for personalisation.</param>
        /// <param name="patientEmail">Patient's real email address, or null/empty to skip email.</param>
        /// <param name="patientPhone">Patient's real phone number (E.164), or null/empty to skip SMS.</param>
        /// <param name="prescriptionSummary">Human-readable list of prescribed medicines.</param>
        /// <param name="channel">"Email", "SMS", or "Both".</param>
        /// <returns>
        /// True if every requested channel succeeded.
        /// False if any requested channel failed or had no valid contact detail.
        /// </returns>
        Task<(bool Success, string? FailureReason)> DispatchPrescriptionNotificationAsync(
            string patientName,
            string? patientEmail,
            string? patientPhone,
            string prescriptionSummary,
            string channel = "Both");
    }

    public class NotificationService : INotificationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<NotificationService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public NotificationService(ILogger<NotificationService> logger)
            : this(new HttpClient(), new ConfigurationBuilder().Build(), logger)
        {
        }

        // ── SendEmailAsync ────────────────────────────────────────────────────

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string messageBody)
        {
            if (string.IsNullOrWhiteSpace(toEmail) || !toEmail.Contains('@'))
            {
                _logger.LogWarning("Email notification skipped: invalid or missing recipient address '{Address}'.", toEmail);
                return false;
            }

            var endpoint = _configuration["Notifications:EmailEndpoint"];
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                _logger.LogError(
                    "Email notification cannot be dispatched: 'Notifications:EmailEndpoint' is not configured. " +
                    "Set this value in appsettings or environment variables to enable email delivery.");
                return false;
            }

            try
            {
                var apiKey = _configuration["Notifications:ApiKey"];

                using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
                {
                    Content = JsonContent.Create(new
                    {
                        to      = toEmail,
                        subject = subject,
                        body    = messageBody
                    })
                };

                if (!string.IsNullOrWhiteSpace(apiKey))
                    request.Headers.Add("Authorization", $"Bearer {apiKey}");

                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("[NotificationService] Email notification dispatched successfully to '{Address}'.", toEmail);
                    return true;
                }

                _logger.LogWarning(
                    "Email dispatch to '{Address}' failed with provider HTTP status {StatusCode}.",
                    toEmail, (int)response.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "Email dispatch to '{Address}' failed due to provider or network error: {ErrorType}",
                    toEmail, ex.GetType().Name);
                return false;
            }
        }

        // ── SendSmsAsync ──────────────────────────────────────────────────────

        public async Task<bool> SendSmsAsync(string phoneNumber, string message)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber) || !phoneNumber.StartsWith("+"))
            {
                _logger.LogWarning(
                    "SMS notification skipped: invalid or missing phone number '{Phone}'. " +
                    "Phone numbers must be in E.164 format (e.g. +94771234567).", phoneNumber);
                return false;
            }

            var endpoint = _configuration["Notifications:SmsEndpoint"];
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                _logger.LogError(
                    "SMS notification cannot be dispatched: 'Notifications:SmsEndpoint' is not configured. " +
                    "Set this value in appsettings or environment variables to enable SMS delivery.");
                return false;
            }

            try
            {
                var apiKey = _configuration["Notifications:ApiKey"];

                using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
                {
                    Content = JsonContent.Create(new
                    {
                        to      = phoneNumber,
                        message = message
                    })
                };

                if (!string.IsNullOrWhiteSpace(apiKey))
                    request.Headers.Add("Authorization", $"Bearer {apiKey}");

                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("[NotificationService] SMS notification dispatched successfully to '{Phone}'.", phoneNumber);
                    return true;
                }

                _logger.LogWarning(
                    "SMS dispatch to '{Phone}' failed with provider HTTP status {StatusCode}.",
                    phoneNumber, (int)response.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "SMS dispatch to '{Phone}' failed due to provider or network error: {ErrorType}",
                    phoneNumber, ex.GetType().Name);
                return false;
            }
        }

        // ── DispatchPrescriptionNotificationAsync ─────────────────────────────

        public async Task<(bool Success, string? FailureReason)> DispatchPrescriptionNotificationAsync(
            string patientName,
            string? patientEmail,
            string? patientPhone,
            string prescriptionSummary,
            string channel = "Both")
        {
            bool wantsEmail = channel is "Email" or "Both";
            bool wantsSms   = channel is "SMS"   or "Both";

            // ── Validate contact availability before attempting dispatch ───────
            bool hasEmail = !string.IsNullOrWhiteSpace(patientEmail) && patientEmail.Contains('@');
            bool hasPhone = !string.IsNullOrWhiteSpace(patientPhone) && patientPhone.StartsWith("+");

            if (wantsEmail && !hasEmail)
            {
                _logger.LogWarning(
                    "Prescription notification: email channel requested but patient '{Name}' has no valid email address on record.",
                    patientName);
            }

            if (wantsSms && !hasPhone)
            {
                _logger.LogWarning(
                    "Prescription notification: SMS channel requested but patient '{Name}' has no valid phone number on record.",
                    patientName);
            }

            // If no valid channel is available, fail immediately
            if ((wantsEmail && !hasEmail) && (wantsSms && !hasPhone))
            {
                var missingMsg = "Patient has no valid email or phone number on record. Cannot dispatch notification.";
                _logger.LogError("Prescription notification failed: {Reason}", missingMsg);
                return (false, missingMsg);
            }

            string emailSubject = "CareFlow AI — E-Prescription Issued & Ready";
            string emailBody    =
                $"Dear {patientName},\n\nYour doctor has issued your e-prescription.\n\n" +
                $"Prescription Summary:\n{prescriptionSummary}\n\n" +
                "Please present your digital prescription in your CareFlow mobile app to the hospital pharmacy for dispensing.\n\n" +
                "CareFlow Hospital Pharmacy Automated System.";

            string smsBody = $"CareFlow AI: Hello {patientName}, your e-prescription has been issued and approved. Check your CareFlow mobile app.";

            bool emailSuccess = !wantsEmail || !hasEmail; // skip ⇒ treated as non-blocking only if SMS still runs
            bool smsSuccess   = !wantsSms   || !hasPhone;

            var failures = new System.Collections.Generic.List<string>();

            if (wantsEmail && hasEmail)
            {
                var emailEndpoint = _configuration["Notifications:EmailEndpoint"];
                if (string.IsNullOrWhiteSpace(emailEndpoint))
                {
                    _logger.LogError("Email notification cannot be dispatched: 'Notifications:EmailEndpoint' is not configured.");
                    failures.Add("Email (Missing configuration: Notifications:EmailEndpoint)");
                }
                else
                {
                    emailSuccess = await SendEmailAsync(patientEmail!, emailSubject, emailBody);
                    if (!emailSuccess) failures.Add("Email");
                }
            }

            if (wantsSms && hasPhone)
            {
                var smsEndpoint = _configuration["Notifications:SmsEndpoint"];
                if (string.IsNullOrWhiteSpace(smsEndpoint))
                {
                    _logger.LogError("SMS notification cannot be dispatched: 'Notifications:SmsEndpoint' is not configured.");
                    failures.Add("SMS (Missing configuration: Notifications:SmsEndpoint)");
                }
                else
                {
                    smsSuccess = await SendSmsAsync(patientPhone!, smsBody);
                    if (!smsSuccess) failures.Add("SMS");
                }
            }

            // Determine overall success:
            // "Both" → both attempted channels must succeed (skip channel if no contact = partial failure)
            bool overallSuccess = channel switch
            {
                "Email" => wantsEmail && hasEmail && emailSuccess,
                "SMS"   => wantsSms   && hasPhone && smsSuccess,
                _       => (wantsEmail ? hasEmail && emailSuccess : true) &&
                           (wantsSms   ? hasPhone && smsSuccess   : true)
            };

            if (overallSuccess)
            {
                _logger.LogInformation(
                    "[NotificationService] Prescription notification dispatched successfully via channel: {Channel} for patient '{Name}'.",
                    channel, patientName);
                return (true, null);
            }

            string? failureReason = failures.Count > 0
                ? $"Notification delivery failed for channel(s): {string.Join(", ", failures)}."
                : "One or more requested channels had no valid contact detail on the patient record.";

            _logger.LogWarning(
                "Prescription notification dispatch failed or partially failed for patient '{Name}'. Reason: {Reason}",
                patientName, failureReason);

            return (false, failureReason);
        }
    }
}
