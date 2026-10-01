using CareFlowAI.API.Data;
using CareFlowAI.API.DTOs;
using CareFlowAI.API.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CareFlowAI.API.Services
{
    public class AppointmentService
    {
        private readonly ApplicationDbContext _context;

        public AppointmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get all appointments
        public async Task<List<AppointmentDto>> GetAllAsync()
        {
            return await _context.Appointments
                .Include(a => a.Doctor)
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .Select(a => new AppointmentDto
                {
                    Id = a.Id,
                    DoctorId = a.DoctorId,
                    DoctorName = a.Doctor.FullName,
                    PatientId = a.PatientId,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    Status = a.Status,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();
        }

        // Get appointment by ID
        public async Task<AppointmentDto?> GetByIdAsync(Guid id)
        {
            return await _context.Appointments
                .Include(a => a.Doctor)
                .Where(a => a.Id == id)
                .Select(a => new AppointmentDto
                {
                    Id = a.Id,
                    DoctorId = a.DoctorId,
                    DoctorName = a.Doctor.FullName,
                    PatientId = a.PatientId,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    Status = a.Status,
                    CreatedAt = a.CreatedAt
                })
                .FirstOrDefaultAsync();
        }

        // Get appointments for a specific patient
        public async Task<List<AppointmentDto>> GetByPatientIdAsync(Guid patientId)
        {
            return await _context.Appointments
                .Include(a => a.Doctor)
                .Where(a => a.PatientId == patientId)
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .Select(a => new AppointmentDto
                {
                    Id = a.Id,
                    DoctorId = a.DoctorId,
                    DoctorName = a.Doctor.FullName,
                    PatientId = a.PatientId,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    Status = a.Status,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();
        }

        // Check whether a booking conflicts with an existing appointment
        public async Task<bool> CheckConflictAsync(
            Guid doctorId,
            DateOnly appointmentDate,
            TimeOnly startTime,
            TimeOnly endTime)
        {
            return await _context.Appointments.AnyAsync(a =>
                a.DoctorId == doctorId &&
                a.AppointmentDate == appointmentDate &&
                a.Status != "Cancelled" &&
                startTime < a.EndTime &&
                endTime > a.StartTime);
        }

        // Create a tentative appointment
        public async Task<AppointmentDto> CreateTentativeAsync(
            CreateAppointmentDto dto)
        {
            // Validate appointment time
            if (dto.StartTime >= dto.EndTime)
            {
                throw new ArgumentException(
                    "Start time must be earlier than end time.");
            }

            // Use a Serializable transaction so concurrent booking
            // requests cannot both pass the conflict check.
            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    System.Data.IsolationLevel.Serializable);

            try
            {
                // Check that the doctor exists and is active
                var doctor = await _context.Doctors
                    .FirstOrDefaultAsync(d =>
                        d.Id == dto.DoctorId &&
                        d.IsActive);

                if (doctor == null)
                {
                    throw new KeyNotFoundException(
                        "Active doctor not found.");
                }

                // Check that the patient exists
                var patientExists = await _context.PatientProfiles
                    .AnyAsync(p => p.Id == dto.PatientId);

                if (!patientExists)
                {
                    throw new KeyNotFoundException(
                        "Patient not found.");
                }

                // Check whether requested time is inside doctor availability
                var hasAvailability = await _context.DoctorAvailabilities
                    .AnyAsync(a =>
                        a.DoctorId == dto.DoctorId &&
                        a.Date == dto.AppointmentDate &&
                        dto.StartTime >= a.StartTime &&
                        dto.EndTime <= a.EndTime);

                if (!hasAvailability)
                {
                    throw new InvalidOperationException(
                        "The selected time is outside the doctor's availability.");
                }

                // Check for an existing conflicting appointment
                var hasConflict = await CheckConflictAsync(
                    dto.DoctorId,
                    dto.AppointmentDate,
                    dto.StartTime,
                    dto.EndTime);

                if (hasConflict)
                {
                    throw new InvalidOperationException(
                        "The selected appointment time is already booked.");
                }

                // Create tentative appointment
                var appointment = new Appointment
                {
                    DoctorId = dto.DoctorId,
                    PatientId = dto.PatientId,
                    AppointmentDate = dto.AppointmentDate,
                    StartTime = dto.StartTime,
                    EndTime = dto.EndTime,
                    Status = "Tentative",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Appointments.Add(appointment);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return new AppointmentDto
                {
                    Id = appointment.Id,
                    DoctorId = appointment.DoctorId,
                    DoctorName = doctor.FullName,
                    PatientId = appointment.PatientId,
                    AppointmentDate = appointment.AppointmentDate,
                    StartTime = appointment.StartTime,
                    EndTime = appointment.EndTime,
                    Status = appointment.Status,
                    CreatedAt = appointment.CreatedAt
                };
            }
            catch (PostgresException ex) when (ex.SqlState == "40001")
            {
                // PostgreSQL serialization failure.
                // Another concurrent transaction won the booking race.
                throw new InvalidOperationException(
                    "The selected appointment time is already booked.");
            }
        }

        // Confirm a tentative appointment
        public async Task<AppointmentDto?> ConfirmAsync(Guid id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                return null;
            }

            if (appointment.Status == "Cancelled")
            {
                throw new InvalidOperationException(
                    "Cancelled appointments cannot be confirmed.");
            }

            appointment.Status = "Confirmed";

            await _context.SaveChangesAsync();

            return new AppointmentDto
            {
                Id = appointment.Id,
                DoctorId = appointment.DoctorId,
                DoctorName = appointment.Doctor.FullName,
                PatientId = appointment.PatientId,
                AppointmentDate = appointment.AppointmentDate,
                StartTime = appointment.StartTime,
                EndTime = appointment.EndTime,
                Status = appointment.Status,
                CreatedAt = appointment.CreatedAt
            };
        }

        // Cancel an appointment
        public async Task<AppointmentDto?> CancelAsync(Guid id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                return null;
            }

            appointment.Status = "Cancelled";

            await _context.SaveChangesAsync();

            return new AppointmentDto
            {
                Id = appointment.Id,
                DoctorId = appointment.DoctorId,
                DoctorName = appointment.Doctor.FullName,
                PatientId = appointment.PatientId,
                AppointmentDate = appointment.AppointmentDate,
                StartTime = appointment.StartTime,
                EndTime = appointment.EndTime,
                Status = appointment.Status,
                CreatedAt = appointment.CreatedAt
            };
        }

        // Delete an appointment
        public async Task<bool> DeleteAsync(Guid id)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                return false;
            }

            _context.Appointments.Remove(appointment);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}