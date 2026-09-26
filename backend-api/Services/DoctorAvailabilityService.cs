using CareFlowAI.API.Data;
using CareFlowAI.API.DTOs;
using CareFlowAI.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CareFlowAI.API.Services
{
    public class DoctorAvailabilityService
    {
        private readonly ApplicationDbContext _context;

        public DoctorAvailabilityService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get all doctor availability records
        public async Task<List<DoctorAvailabilityDto>> GetAllAsync()
        {
            return await _context.DoctorAvailabilities
                .Include(a => a.Doctor)
                .Select(a => new DoctorAvailabilityDto
                {
                    Id = a.Id,
                    DoctorId = a.DoctorId,
                    DoctorName = a.Doctor.FullName,
                    Specialization = a.Doctor.Specialization,
                    Date = a.Date,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime
                })
                .OrderBy(a => a.Date)
                .ThenBy(a => a.StartTime)
                .ToListAsync();
        }

        // Get availability records for a specific doctor
        public async Task<List<DoctorAvailabilityDto>> GetByDoctorIdAsync(Guid doctorId)
        {
            return await _context.DoctorAvailabilities
                .Include(a => a.Doctor)
                .Where(a => a.DoctorId == doctorId)
                .Select(a => new DoctorAvailabilityDto
                {
                    Id = a.Id,
                    DoctorId = a.DoctorId,
                    DoctorName = a.Doctor.FullName,
                    Specialization = a.Doctor.Specialization,
                    Date = a.Date,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime
                })
                .OrderBy(a => a.Date)
                .ThenBy(a => a.StartTime)
                .ToListAsync();
        }

        // Create a new availability record
        public async Task<DoctorAvailabilityDto?> CreateAsync(
            CreateDoctorAvailabilityDto dto)
        {
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.Id == dto.DoctorId && d.IsActive);

            if (doctor == null)
            {
                return null;
            }

            if (dto.StartTime >= dto.EndTime)
            {
                throw new ArgumentException(
                    "Start time must be earlier than end time.");
            }

            var overlappingAvailability =
                await _context.DoctorAvailabilities.AnyAsync(a =>
                    a.DoctorId == dto.DoctorId &&
                    a.Date == dto.Date &&
                    dto.StartTime < a.EndTime &&
                    dto.EndTime > a.StartTime);

            if (overlappingAvailability)
            {
                throw new InvalidOperationException(
                    "The doctor already has availability during this time.");
            }

            var availability = new DoctorAvailability
            {
                DoctorId = dto.DoctorId,
                Date = dto.Date,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime
            };

            _context.DoctorAvailabilities.Add(availability);
            await _context.SaveChangesAsync();

            return new DoctorAvailabilityDto
            {
                Id = availability.Id,
                DoctorId = availability.DoctorId,
                DoctorName = doctor.FullName,
                Specialization = doctor.Specialization,
                Date = availability.Date,
                StartTime = availability.StartTime,
                EndTime = availability.EndTime
            };
        }

        // Update an availability record
        public async Task<DoctorAvailabilityDto?> UpdateAsync(
            Guid id,
            UpdateDoctorAvailabilityDto dto)
        {
            var availability = await _context.DoctorAvailabilities
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (availability == null)
            {
                return null;
            }

            if (dto.StartTime >= dto.EndTime)
            {
                throw new ArgumentException(
                    "Start time must be earlier than end time.");
            }

            var overlappingAvailability =
                await _context.DoctorAvailabilities.AnyAsync(a =>
                    a.Id != id &&
                    a.DoctorId == availability.DoctorId &&
                    a.Date == dto.Date &&
                    dto.StartTime < a.EndTime &&
                    dto.EndTime > a.StartTime);

            if (overlappingAvailability)
            {
                throw new InvalidOperationException(
                    "The doctor already has another availability during this time.");
            }

            availability.Date = dto.Date;
            availability.StartTime = dto.StartTime;
            availability.EndTime = dto.EndTime;

            await _context.SaveChangesAsync();

            return new DoctorAvailabilityDto
            {
                Id = availability.Id,
                DoctorId = availability.DoctorId,
                DoctorName = availability.Doctor.FullName,
                Specialization = availability.Doctor.Specialization,
                Date = availability.Date,
                StartTime = availability.StartTime,
                EndTime = availability.EndTime
            };
        }

        // Delete an availability record
        public async Task<bool> DeleteAsync(Guid id)
        {
            var availability = await _context.DoctorAvailabilities
                .FirstOrDefaultAsync(a => a.Id == id);

            if (availability == null)
            {
                return false;
            }

            _context.DoctorAvailabilities.Remove(availability);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}