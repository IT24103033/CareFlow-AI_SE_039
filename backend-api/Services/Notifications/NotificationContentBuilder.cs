using System;
using System.Net;
using CareFlowAI.API.Models;

namespace CareFlowAI.API.Services.Notifications
{
    /// <summary>
    /// Builds minimal notification emails. By design they contain NO symptoms, medical history,
    /// diagnoses or medication details: patients are directed to CareFlow instead.
    /// </summary>
    public static class NotificationContentBuilder
    {
        public sealed record Content(string Subject, string Html, string Text);

        public static Content PrescriptionIssued(string? patientName, string? appUrl) =>
            Build(
                "You have an update in CareFlow",
                Greeting(patientName),
                "A new update is available in your CareFlow account.",
                "Please open the CareFlow app or web portal to view it.",
                appUrl);

        public static Content AppointmentConfirmed(
            string? patientName, DateOnly date, TimeOnly start, string? doctorName, string? appUrl)
        {
            var when = $"{date:dddd, d MMMM yyyy} at {start:HH:mm}";
            var with = string.IsNullOrWhiteSpace(doctorName) ? string.Empty : $" with {doctorName.Trim()}";
            return Build(
                "Your CareFlow appointment is confirmed",
                Greeting(patientName),
                $"Your appointment{with} is confirmed for {when}.",
                "Please check the CareFlow app or web portal for the latest details.",
                appUrl);
        }

        private static string Greeting(string? fullName)
        {
            var first = (fullName ?? string.Empty).Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            return string.IsNullOrEmpty(first) ? "Hello," : $"Hello {first},";
        }

        private static Content Build(string subject, string greeting, string line1, string line2, string? appUrl)
        {
            var link = Uri.TryCreate(appUrl, UriKind.Absolute, out var u) && u.Scheme == Uri.UriSchemeHttps
                ? u.ToString()
                : null;

            var text = $"{greeting}\n\n{line1}\n{line2}" + (link != null ? $"\n\n{link}" : string.Empty)
                       + "\n\n— CareFlow AI\nThis is an automated message. Please do not reply.";

            var html =
                "<div style=\"font-family:Arial,Helvetica,sans-serif;font-size:15px;color:#1f2937;max-width:520px\">" +
                $"<p>{WebUtility.HtmlEncode(greeting)}</p>" +
                $"<p>{WebUtility.HtmlEncode(line1)}<br/>{WebUtility.HtmlEncode(line2)}</p>" +
                (link != null ? $"<p><a href=\"{WebUtility.HtmlEncode(link)}\">Open CareFlow</a></p>" : string.Empty) +
                "<p style=\"color:#6b7280;font-size:12px\">— CareFlow AI<br/>This is an automated message. Please do not reply.</p>" +
                "</div>";

            return new Content(subject, html, text);
        }
    }
}
