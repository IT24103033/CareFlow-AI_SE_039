using System;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using CareFlowAI.API.Data;
using CareFlowAI.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CareFlowAI.API.Services.Notifications
{
    public sealed record RecipientResult(bool Ok, string? Email, string? FailureCode, string? FailureReason)
    {
        public static RecipientResult Valid(string email) => new(true, email, null, null);
        public static RecipientResult Fail(string code, string reason) => new(false, null, code, reason);
    }

    /// <summary>
    /// Resolves the email address for a patient profile. Never matches by patient name.
    /// Order: (1) the Patient user account linked by Users.PatientProfileId, (2) PatientProfiles.Email.
    /// Existing contact data is never modified.
    /// </summary>
    public class NotificationRecipientResolver
    {
        // Placeholder / non-deliverable domains (the app generates "<username>@careflow.ai" for staff accounts).
        private static readonly string[] BlockedDomains =
        {
            "careflow.ai", "example.com", "example.org", "example.net", "localhost", "invalid", "test"
        };

        private readonly ApplicationDbContext _db;

        public NotificationRecipientResolver(ApplicationDbContext db) => _db = db;

        public async Task<RecipientResult> ResolveEmailAsync(Guid patientProfileId, CancellationToken ct = default)
        {
            var accountEmail = await _db.Users.AsNoTracking()
                .Where(u => u.PatientProfileId == patientProfileId && u.Role == "Patient")
                .OrderBy(u => u.CreatedAt)
                .Select(u => u.Email)
                .FirstOrDefaultAsync(ct);

            var profileEmail = await _db.PatientProfiles.AsNoTracking()
                .Where(p => p.Id == patientProfileId)
                .Select(p => p.Email)
                .FirstOrDefaultAsync(ct);

            if (TryNormalize(accountEmail, out var email) || TryNormalize(profileEmail, out email))
                return RecipientResult.Valid(email);

            if (string.IsNullOrWhiteSpace(accountEmail) && string.IsNullOrWhiteSpace(profileEmail))
                return RecipientResult.Fail(NotificationFailureCodes.NoRecipient,
                    "This patient has no email address on record.");

            return RecipientResult.Fail(NotificationFailureCodes.InvalidRecipient,
                "The patient's email address on record is not a valid, deliverable address.");
        }

        public static bool TryNormalize(string? raw, out string email)
        {
            email = string.Empty;
            if (string.IsNullOrWhiteSpace(raw)) return false;

            var candidate = raw.Trim();
            if (candidate.Length > 254 || candidate.Any(c => char.IsWhiteSpace(c) || char.IsControl(c)))
                return false;

            try
            {
                var parsed = new MailAddress(candidate);
                if (!string.Equals(parsed.Address, candidate, StringComparison.OrdinalIgnoreCase)) return false;
                var domain = parsed.Host.ToLowerInvariant();
                if (!domain.Contains('.') || domain.StartsWith('.') || domain.EndsWith('.')) return false;
                if (BlockedDomains.Any(b => domain == b || domain.EndsWith("." + b))) return false;
                email = candidate.ToLowerInvariant();
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public static string Mask(string email)
        {
            var at = email.IndexOf('@');
            if (at <= 0) return "***";
            return email[0] + "***" + email[at..];
        }
    }
}
