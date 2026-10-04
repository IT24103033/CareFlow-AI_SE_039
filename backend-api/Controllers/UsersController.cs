using CareFlowAI.API.Data;
using CareFlowAI.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareFlowAI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private static readonly HashSet<string> AllowedRoles = new(StringComparer.OrdinalIgnoreCase)
        {
            "Admin", "Doctor", "Staff"
        };

        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] string? role)
        {
            var query = _context.Users.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(role))
                query = query.Where(u => u.Role == role);

            var users = await query
                .OrderBy(u => u.Role)
                .ThenBy(u => u.Username)
                .Select(u => new { u.Id, u.Username, u.Role })
                .ToListAsync();

            return Ok(users);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UserUpsertDto dto)
        {
            if (!AllowedRoles.Contains(dto.Role))
                return BadRequest("Role must be Admin, Doctor, or Staff.");

            if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
                return BadRequest("Password must be at least 6 characters.");

            var username = dto.Username.Trim();
            var exists = await _context.Users.AnyAsync(u => EF.Functions.ILike(u.Username, username));
            if (exists)
                return Conflict("A user with that username already exists.");

            var user = new User
            {
                Username = username,
                Password = dto.Password,
                Role = NormalizeRole(dto.Role)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok(new { user.Id, user.Username, user.Role });
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UserUpsertDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound("User not found.");

            if (!AllowedRoles.Contains(dto.Role))
                return BadRequest("Role must be Admin, Doctor, or Staff.");

            var username = dto.Username.Trim();
            var taken = await _context.Users.AnyAsync(u => u.Id != id && EF.Functions.ILike(u.Username, username));
            if (taken)
                return Conflict("A user with that username already exists.");

            user.Username = username;
            user.Role = NormalizeRole(dto.Role);
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                if (dto.Password.Length < 6)
                    return BadRequest("Password must be at least 6 characters.");
                user.Password = dto.Password;
            }

            await _context.SaveChangesAsync();
            return Ok(new { user.Id, user.Username, user.Role });
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound("User not found.");

            if (string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                var adminCount = await _context.Users.CountAsync(u => u.Role == "Admin");
                if (adminCount <= 1)
                    return BadRequest("Cannot remove the last admin account.");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private static string NormalizeRole(string role) =>
            AllowedRoles.First(r => r.Equals(role, StringComparison.OrdinalIgnoreCase));
    }
}
