using System.Security.Claims;
using CareFlowAI.API.Data;
using CareFlowAI.API.DTOs;
using CareFlowAI.API.Models;
using CareFlowAI.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareFlowAI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IJwtTokenService _tokenService;

        public AuthController(ApplicationDbContext context, IJwtTokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        // ── POST api/auth/login ─────────────────────────────────────────────
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Username and password are required.");

            var identifierTrimmed = dto.Username.Trim().ToLower();
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == identifierTrimmed || (u.Email != null && u.Email.ToLower() == identifierTrimmed));

            if (user == null || !PasswordHasher.Verify(dto.Password, user.Password))
            {
                return Unauthorized("Invalid username or password.");
            }

            // Transparently upgrade legacy plain-text password to BCrypt hash
            if (PasswordHasher.IsLegacyPlain(user.Password))
            {
                user.Password = PasswordHasher.Hash(dto.Password);
                await _context.SaveChangesAsync();
            }

            Doctor? doctor = null;
            if (user.Role == "Doctor")
            {
                if (user.DoctorId.HasValue)
                {
                    doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Id == user.DoctorId && d.IsActive);
                }
                else
                {
                    doctor = await _context.Doctors.FirstOrDefaultAsync(d => (d.Email.ToLower() == identifierTrimmed || d.FullName.ToLower() == identifierTrimmed || (user.Email != null && d.Email.ToLower() == user.Email.ToLower())) && d.IsActive);
                    if (doctor != null)
                    {
                        user.DoctorId = doctor.Id;
                        if (string.IsNullOrWhiteSpace(user.Email) && !string.IsNullOrWhiteSpace(doctor.Email))
                        {
                            user.Email = doctor.Email;
                        }
                        await _context.SaveChangesAsync();
                    }
                }
            }

            PatientProfile? patient = null;
            if (user.Role == "Patient")
            {
                if (user.PatientProfileId.HasValue)
                {
                    patient = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.Id == user.PatientProfileId);
                }
                else
                {
                    patient = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.FullName.ToLower() == identifierTrimmed);
                    if (patient != null)
                    {
                        user.PatientProfileId = patient.Id;
                        await _context.SaveChangesAsync();
                    }
                }
            }

            var (token, expiresAt) = _tokenService.GenerateToken(user, doctor, patient);

            return Ok(new AuthResponseDto
            {
                Token = token,
                TokenType = "Bearer",
                ExpiresAt = expiresAt,
                User = new AuthUserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email ?? string.Empty,
                    Role = user.Role,
                    DoctorId = doctor?.Id ?? user.DoctorId,
                    PatientId = patient?.Id ?? user.PatientProfileId,
                    FullName = doctor?.FullName ?? patient?.FullName ?? user.Username
                }
            });
        }

        // ── POST api/auth/register-patient ──────────────────────────────────
        [HttpPost("register-patient")]
        public async Task<IActionResult> RegisterPatient([FromBody] RegisterPatientRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var usernameLower = dto.Username.Trim().ToLower();
            if (await _context.Users.AnyAsync(u => u.Username.ToLower() == usernameLower))
                return Conflict("A user with this username already exists.");

            var emailLower = dto.Email.Trim().ToLower();
            if (await _context.Users.AnyAsync(u => u.Email != null && u.Email.ToLower() == emailLower))
                return Conflict("A user with this email address already exists.");

            // 1. Create PatientProfile
            var patient = new PatientProfile
            {
                FullName = dto.FullName.Trim(),
                DateOfBirth = dto.DateOfBirth,
                BloodGroup = string.IsNullOrWhiteSpace(dto.BloodGroup) ? "O+" : dto.BloodGroup.Trim(),
                MedicalHistorySummary = string.IsNullOrWhiteSpace(dto.MedicalHistorySummary)
                    ? "Registered via Mobile App"
                    : dto.MedicalHistorySummary.Trim(),
                CreatedAt = DateTime.UtcNow
            };
            _context.PatientProfiles.Add(patient);
            await _context.SaveChangesAsync();

            // 2. Create User account with server-assigned 'Patient' role only
            var user = new User
            {
                Username = dto.Username.Trim(),
                Email = emailLower,
                Password = PasswordHasher.Hash(dto.Password),
                Role = "Patient", // Server-enforced: public registration can NEVER grant Admin/Doctor/Staff
                PatientProfileId = patient.Id,
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var (token, expiresAt) = _tokenService.GenerateToken(user, patient: patient);

            return CreatedAtAction(nameof(GetMe), null, new AuthResponseDto
            {
                Token = token,
                TokenType = "Bearer",
                ExpiresAt = expiresAt,
                User = new AuthUserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role,
                    PatientId = patient.Id,
                    FullName = patient.FullName
                }
            });
        }

        // ── POST api/auth/register-staff ────────────────────────────────────
        [Authorize(Roles = "Admin")]
        [HttpPost("register-staff")]
        public async Task<IActionResult> RegisterStaff([FromBody] RegisterStaffRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.Role != "Staff" && dto.Role != "Doctor" && dto.Role != "Admin")
                return BadRequest("Role must be 'Staff', 'Doctor', or 'Admin'.");

            var usernameLower = dto.Username.Trim().ToLower();
            if (await _context.Users.AnyAsync(u => u.Username.ToLower() == usernameLower))
                return Conflict("A user with this username already exists.");

            Doctor? doctor = null;
            if (dto.Role == "Doctor")
            {
                doctor = new Doctor
                {
                    FullName = string.IsNullOrWhiteSpace(dto.FullName) ? dto.Username : dto.FullName.Trim(),
                    Specialization = string.IsNullOrWhiteSpace(dto.Specialization) ? "General Practitioner" : dto.Specialization.Trim(),
                    Email = string.IsNullOrWhiteSpace(dto.Email) ? $"{dto.Username}@careflow.ai" : dto.Email.Trim(),
                    IsActive = true
                };
                _context.Doctors.Add(doctor);
                await _context.SaveChangesAsync();
            }

            var user = new User
            {
                Username = dto.Username.Trim(),
                FullName = string.IsNullOrWhiteSpace(dto.FullName) ? dto.Username : dto.FullName.Trim(),
                Email = string.IsNullOrWhiteSpace(dto.Email) ? $"{dto.Username}@careflow.ai" : dto.Email.Trim().ToLower(),
                Password = PasswordHasher.Hash(dto.Password),
                Role = dto.Role,
                DoctorId = doctor?.Id,
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                id = user.Id,
                username = user.Username,
                email = user.Email,
                role = user.Role,
                doctorId = doctor?.Id
            });
        }

        // ── GET api/auth/me ─────────────────────────────────────────────────
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;

            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            Doctor? doctor = null;
            if (user.DoctorId.HasValue)
            {
                doctor = await _context.Doctors.FindAsync(user.DoctorId.Value);
            }

            PatientProfile? patient = null;
            if (user.PatientProfileId.HasValue)
            {
                patient = await _context.PatientProfiles.FindAsync(user.PatientProfileId.Value);
            }

            return Ok(new AuthUserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email ?? string.Empty,
                Role = user.Role,
                DoctorId = doctor?.Id ?? user.DoctorId,
                PatientId = patient?.Id ?? user.PatientProfileId,
                FullName = doctor?.FullName ?? patient?.FullName ?? user.Username
            });
        }
        // ── GET api/auth/staff ──────────────────────────────────────────────
        [Authorize(Roles = "Admin")]
        [HttpGet("staff")]
        public async Task<IActionResult> GetStaff()
        {
            var staff = await _context.Users
                .AsNoTracking()
                .Where(u => u.Role == "Admin" || u.Role == "Doctor" || u.Role == "Staff")
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    Email = u.Email ?? "",
                    u.Role,
                    u.DoctorId
                })
                .ToListAsync();

            return Ok(staff);
        }
    }
}
