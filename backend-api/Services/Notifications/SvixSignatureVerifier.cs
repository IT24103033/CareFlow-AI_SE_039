using System;
using System.Security.Cryptography;
using System.Text;

namespace CareFlowAI.API.Services.Notifications
{
    /// <summary>
    /// Verifies Svix-style webhook signatures (used by Resend):
    /// signature = base64(HMAC-SHA256(base64decode(secret without "whsec_"), "{svix-id}.{svix-timestamp}.{rawBody}")).
    /// The header may hold several space-separated "v1,&lt;sig&gt;" entries.
    /// </summary>
    public static class SvixSignatureVerifier
    {
        public static bool Verify(
            string secret, string? id, string? timestamp, string? signatureHeader, string rawBody,
            DateTimeOffset now, TimeSpan tolerance)
        {
            if (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(id)
                || string.IsNullOrWhiteSpace(timestamp) || string.IsNullOrWhiteSpace(signatureHeader))
                return false;

            if (!long.TryParse(timestamp, out var seconds)) return false;
            var sent = DateTimeOffset.FromUnixTimeSeconds(seconds);
            if ((now - sent).Duration() > tolerance) return false; // stale or too far in the future (replay protection)

            byte[] key;
            try
            {
                var b64 = secret.StartsWith("whsec_", StringComparison.Ordinal) ? secret["whsec_".Length..] : secret;
                key = Convert.FromBase64String(b64);
            }
            catch (FormatException)
            {
                return false;
            }

            using var hmac = new HMACSHA256(key);
            var expected = hmac.ComputeHash(Encoding.UTF8.GetBytes($"{id}.{timestamp}.{rawBody}"));

            foreach (var part in signatureHeader.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                var comma = part.IndexOf(',');
                if (comma <= 0 || part[..comma] != "v1") continue;
                try
                {
                    var provided = Convert.FromBase64String(part[(comma + 1)..]);
                    if (CryptographicOperations.FixedTimeEquals(expected, provided)) return true;
                }
                catch (FormatException)
                {
                    // ignore malformed candidate signature
                }
            }
            return false;
        }

        /// <summary>Test helper / tooling: produce a header value for the given payload.</summary>
        public static string Sign(string secret, string id, string timestamp, string rawBody)
        {
            var b64 = secret.StartsWith("whsec_", StringComparison.Ordinal) ? secret["whsec_".Length..] : secret;
            using var hmac = new HMACSHA256(Convert.FromBase64String(b64));
            return "v1," + Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes($"{id}.{timestamp}.{rawBody}")));
        }
    }
}
