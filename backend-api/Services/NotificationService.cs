using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CareFlowAI.API.Services
{
    /// <summary>
    /// Component D – Third-Party Notification Service (SMS & Email)
    /// Dispatches notifications via external HTTP providers when e-prescriptions are issued or approved.
    /// </summary>
    public interface INotificationService
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string messageBody);
        Task<bool> SendSmsAsync(string phoneNumber, string message);
        Task<bool> DispatchPrescriptionNotificationAsync(string patientName, string contact, string prescriptionSummary, string channel = "Both");
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

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string messageBody)
        {
            try
            {
                var endpoint = _configuration["Notifications:EmailEndpoint"]
                               ?? "https://api.careflow.hospital.org/notifications/email";
                var apiKey = _configuration["Notifications:ApiKey"];

                using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
                {
                    Content = JsonContent.Create(new
                    {
                        to = toEmail,
                        subject = subject,
                        body = messageBody
                    })
                };

                if (!string.IsNullOrWhiteSpace(apiKey))
                {
                    request.Headers.Add("Authorization", $"Bearer {apiKey}");
                }

                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Email notification dispatched successfully.");
                    return true;
                }

                _logger.LogWarning("Email dispatch failed with provider HTTP status {StatusCode}.", (int)response.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError("Email dispatch failed due to provider or network error: {ErrorType}", ex.GetType().Name);
                return false;
            }
        }

        public async Task<bool> SendSmsAsync(string phoneNumber, string message)
        {
            try
            {
                var endpoint = _configuration["Notifications:SmsEndpoint"]
                               ?? "https://api.careflow.hospital.org/notifications/sms";
                var apiKey = _configuration["Notifications:ApiKey"];

                using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
                {
                    Content = JsonContent.Create(new
                    {
                        to = phoneNumber,
                        message = message
                    })
                };

                if (!string.IsNullOrWhiteSpace(apiKey))
                {
                    request.Headers.Add("Authorization", $"Bearer {apiKey}");
                }

                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("SMS notification dispatched successfully.");
                    return true;
                }

                _logger.LogWarning("SMS dispatch failed with provider HTTP status {StatusCode}.", (int)response.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError("SMS dispatch failed due to provider or network error: {ErrorType}", ex.GetType().Name);
                return false;
            }
        }

        public async Task<bool> DispatchPrescriptionNotificationAsync(string patientName, string contact, string prescriptionSummary, string channel = "Both")
        {
            string email = contact.Contains("@") ? contact : "patient@careflow.hospital.org";
            string phone = contact.StartsWith("+") ? contact : "+15550198372";

            string emailSubject = "CareFlow AI — E-Prescription Issued & Ready";
            string emailBody = $"Dear {patientName},\n\nYour doctor has issued your e-prescription.\n\nPrescription Summary:\n{prescriptionSummary}\n\n" +
                               "Please present your digital prescription in your CareFlow mobile app to the hospital pharmacy for dispensing.\n\n" +
                               "CareFlow Hospital Pharmacy Automated System.";

            string smsBody = $"CareFlow AI: Hello {patientName}, your e-prescription has been issued and approved. Check your CareFlow mobile app.";

            bool emailSuccess = true;
            bool smsSuccess = true;

            if (channel is "Email" or "Both")
            {
                emailSuccess = await SendEmailAsync(email, emailSubject, emailBody);
            }
            if (channel is "SMS" or "Both")
            {
                smsSuccess = await SendSmsAsync(phone, smsBody);
            }

            bool overallSuccess = channel switch
            {
                "Email" => emailSuccess,
                "SMS"   => smsSuccess,
                _       => emailSuccess && smsSuccess
            };

            if (overallSuccess)
            {
                _logger.LogInformation("Prescription notification dispatched successfully via channel: {Channel}.", channel);
            }
            else
            {
                _logger.LogWarning("Prescription notification dispatch failed or partially failed for channel: {Channel}.", channel);
            }

            return overallSuccess;
        }
    }
}
