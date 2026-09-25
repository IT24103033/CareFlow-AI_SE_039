using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CareFlowAI.API.Services
{
    /// <summary>
    /// Component D – Third-Party Notification Service (SMS & Email)
    /// Dispatches notifications when e-prescriptions are issued, approved, or when low-stock alerts occur.
    /// </summary>
    public interface INotificationService
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string messageBody);
        Task<bool> SendSmsAsync(string phoneNumber, string message);
        Task<bool> DispatchPrescriptionNotificationAsync(string patientName, string contact, string prescriptionSummary, string channel = "Both");
    }

    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ILogger<NotificationService> logger)
        {
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string messageBody)
        {
            await Task.Delay(100); // Simulate network round-trip to SendGrid / AWS SES
            _logger.LogInformation("📧 [THIRD-PARTY EMAIL DISPATCHED] To: {To} | Subject: {Subject} | Body: {Body}", toEmail, subject, messageBody);
            return true;
        }

        public async Task<bool> SendSmsAsync(string phoneNumber, string message)
        {
            await Task.Delay(100); // Simulate network round-trip to Twilio SMS Gateway
            _logger.LogInformation("📱 [THIRD-PARTY SMS DISPATCHED] To: {Phone} | Message: {Message}", phoneNumber, message);
            return true;
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

            bool success = true;
            if (channel == "Email" || channel == "Both")
            {
                success &= await SendEmailAsync(email, emailSubject, emailBody);
            }
            if (channel == "SMS" || channel == "Both")
            {
                success &= await SendSmsAsync(phone, smsBody);
            }

            return success;
        }
    }
}
