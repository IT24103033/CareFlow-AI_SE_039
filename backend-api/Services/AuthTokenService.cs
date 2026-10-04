using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace CareFlowAI.API.Services
{
    public class AuthTokenService
    {
        private readonly IConfiguration _configuration;

        public AuthTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string CreateToken(Guid userId, string username, string role)
        {
            var jwt = _configuration.GetSection("Jwt");
            var secret = jwt["Secret"] ?? "CareFlowAI_Dev_Only_Signing_Key_Change_Me_32";
            var issuer = jwt["Issuer"] ?? "CareFlowAI";
            var audience = jwt["Audience"] ?? "CareFlowAI.Clients";
            var minutes = jwt.GetValue("ExpiryMinutes", 120);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
            };

            var token = new JwtSecurityToken(
                issuer,
                audience,
                claims,
                expires: DateTime.UtcNow.AddMinutes(minutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
