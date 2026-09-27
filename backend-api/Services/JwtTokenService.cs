using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CareFlowAI.API.Models;
using Microsoft.IdentityModel.Tokens;

namespace CareFlowAI.API.Services
{
    public interface IJwtTokenService
    {
        (string Token, DateTime ExpiresAt) GenerateToken(User user, Doctor? doctor = null, PatientProfile? patient = null);
    }

    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _config;

        public JwtTokenService(IConfiguration config)
        {
            _config = config;
        }

        public (string Token, DateTime ExpiresAt) GenerateToken(User user, Doctor? doctor = null, PatientProfile? patient = null)
        {
            var secret = _config["Jwt:Secret"] ?? "CareFlowAI_Super_Secret_Key_For_Jwt_Signing_Must_Be_Long_Enough_2026!";
            var issuer = _config["Jwt:Issuer"] ?? "CareFlowAI";
            var audience = _config["Jwt:Audience"] ?? "CareFlowAI.Clients";
            var expiryMinutes = int.TryParse(_config["Jwt:ExpiryMinutes"], out var exp) ? exp : 120;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("role", user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
                claims.Add(new Claim(ClaimTypes.Email, user.Email));
            }

            if (user.Role == "Doctor")
            {
                var docId = doctor?.Id ?? user.DoctorId;
                if (docId.HasValue && docId.Value != Guid.Empty)
                {
                    claims.Add(new Claim("doctor_id", docId.Value.ToString()));
                }
            }

            if (user.Role == "Patient")
            {
                var patId = patient?.Id ?? user.PatientProfileId;
                if (patId.HasValue && patId.Value != Guid.Empty)
                {
                    claims.Add(new Claim("patient_id", patId.Value.ToString()));
                }
            }

            var fullName = doctor?.FullName ?? patient?.FullName ?? user.Username;
            claims.Add(new Claim("full_name", fullName));

            var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiresAt,
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(securityToken);

            return (tokenString, expiresAt);
        }
    }
}
