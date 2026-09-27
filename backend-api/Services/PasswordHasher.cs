using BCrypt.Net;

namespace CareFlowAI.API.Services
{
    public static class PasswordHasher
    {
        private const int WorkFactor = 11;

        public static string Hash(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty.", nameof(password));

            return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
        }

        public static bool Verify(string password, string storedPassword)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedPassword))
                return false;

            try
            {
                if (storedPassword.StartsWith("$2a$") ||
                    storedPassword.StartsWith("$2b$") ||
                    storedPassword.StartsWith("$2y$"))
                {
                    return BCrypt.Net.BCrypt.Verify(password, storedPassword);
                }
            }
            catch
            {
                // In case of corrupt hash format
            }

            // Backward compatibility with unhashed dev seed passwords ("password")
            return password == storedPassword;
        }

        public static bool IsLegacyPlain(string storedPassword)
        {
            return !(storedPassword.StartsWith("$2a$") ||
                     storedPassword.StartsWith("$2b$") ||
                     storedPassword.StartsWith("$2y$"));
        }
    }
}
