using CareFlowAI.API.Data;
using CareFlowAI.API.Models;
using CareFlowAI.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareFlowAI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly AuthTokenService _tokens;

        public AuthController(ApplicationDbContext context, AuthTokenService tokens)
        {
            _context = context;
            _tokens = tokens;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var raw = (request.Username ?? string.Empty).Trim();
            var lookup = raw.Contains('@', StringComparison.Ordinal) ? raw.Split('@')[0] : raw;
            var password = (request.Password ?? string.Empty).Trim();

            var users = await _context.Users.AsNoTracking().ToListAsync();
            var user = users.FirstOrDefault(u =>
            {
                var stored = (u.Username ?? string.Empty).Trim();
                var storedLocal = stored.Contains('@', StringComparison.Ordinal) ? stored.Split('@')[0] : stored;
                return stored.Equals(raw, StringComparison.OrdinalIgnoreCase)
                    || stored.Equals(lookup, StringComparison.OrdinalIgnoreCase)
                    || storedLocal.Equals(lookup, StringComparison.OrdinalIgnoreCase);
            });

            if (user == null || !string.Equals((user.Password ?? string.Empty).Trim(), password, StringComparison.Ordinal))
                return Unauthorized(new { error = "Invalid username or password." });

            var token = _tokens.CreateToken(user.Id, user.Username, user.Role);
            return Ok(new
            {
                token,
                username = user.Username,
                role = user.Role
            });
        }
    }
}
