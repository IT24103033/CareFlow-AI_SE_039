using CareFlowAI.API.Data;
using CareFlowAI.API.DTOs;
using CareFlowAI.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace CareFlowAI.API.Tests;

public class AppointmentSchedulingTests
{
    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var db = new ApplicationDbContext(options);
        db.Database.EnsureCreated();
        
        // Seed basic data required by tests
        if (!db.Doctors.Any())
        {
            var doctor = new CareFlowAI.API.Models.Doctor
            {
                Id = Guid.NewGuid(),
                FullName = "Test Doctor",
                Specialization = "General",
                Email = "doctor@test.com"
            };
            db.Doctors.Add(doctor);
            
            var patient = new CareFlowAI.API.Models.PatientProfile
            {
                Id = Guid.NewGuid(),
                FullName = "Test Patient",
                DateOfBirth = new DateOnly(1990, 1, 1),
                CreatedAt = DateTime.UtcNow
            };
            db.PatientProfiles.Add(patient);
            
            var availability = new CareFlowAI.API.Models.DoctorAvailability
            {
                Id = Guid.NewGuid(),
                DoctorId = doctor.Id,
                Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                StartTime = new TimeOnly(9, 0, 0),
                EndTime = new TimeOnly(17, 0, 0)
            };
            db.DoctorAvailabilities.Add(availability);
            
            var appointment = new CareFlowAI.API.Models.Appointment
            {
                Id = Guid.NewGuid(),
                DoctorId = doctor.Id,
                PatientId = patient.Id,
                AppointmentDate = availability.Date,
                StartTime = new TimeOnly(10, 0, 0),
                EndTime = new TimeOnly(10, 30, 0),
                Status = "Tentative",
                ApprovalStatus = "Pending"
            };
            db.Appointments.Add(appointment);
            
            db.SaveChanges();
        }
        
        return db;
    }

    [Fact]
    public async Task ConfirmAppointment_ShouldFail_WhenNotApproved()
    {
        await using var db = CreateDbContext();

        var appointment = await db.Appointments
            .FirstOrDefaultAsync();

        if (appointment == null)
        {
            return;
        }

        appointment.Status = "Tentative";
        appointment.ApprovalStatus = "Pending";

        await db.SaveChangesAsync();

        var service = new AppointmentService(db);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ConfirmAsync(appointment.Id));
    }

    [Fact]
    public async Task Appointment_ShouldBeConfirmed_AfterDoctorApproval()
    {
        await using var db = CreateDbContext();

        var appointment = await db.Appointments
            .FirstOrDefaultAsync();

        if (appointment == null)
        {
            return;
        }

        appointment.Status = "Tentative";
        appointment.ApprovalStatus = "Pending";
        appointment.ApprovedByDoctorId = null;
        appointment.ApprovedAt = null;

        await db.SaveChangesAsync();

        var service = new AppointmentService(db);

        var approved = await service.ApproveAsync(
            appointment.Id,
            appointment.DoctorId);

        Assert.NotNull(approved);

        Assert.Equal(
            "Approved",
            approved!.ApprovalStatus);

        var confirmed = await service.ConfirmAsync(
            appointment.Id);

        Assert.NotNull(confirmed);

        Assert.Equal(
            "Confirmed",
            confirmed!.Status);
    }

    [Fact]
    public async Task Appointment_ShouldDetectExistingConflict()
    {
        await using var db = CreateDbContext();

        var existing = await db.Appointments
            .FirstOrDefaultAsync(
                a => a.Status != "Cancelled");

        if (existing == null)
        {
            return;
        }

        var service = new AppointmentService(db);

        var conflict = await service.CheckConflictAsync(
            existing.DoctorId,
            existing.AppointmentDate,
            existing.StartTime,
            existing.EndTime);

        Assert.True(conflict);
    }

    [Fact]
    public async Task DoctorAvailabilityService_ShouldReturnSlots()
    {
        await using var db = CreateDbContext();

        var doctor = await db.Doctors
            .FirstOrDefaultAsync();

        if (doctor == null)
        {
            return;
        }

        var service = new DoctorAvailabilityService(db);

        var date = DateOnly.FromDateTime(
            DateTime.UtcNow.Date.AddDays(30));

        var slots = await service.GetAvailableSlotsAsync(
            doctor.Id,
            date,
            30);

        Assert.NotNull(slots);
    }

    [Fact]
    public async Task Appointment_ShouldReturnUnavailable_WhenNoAvailabilityExists()
    {
        await using var db = CreateDbContext();

        var doctor = await db.Doctors
            .FirstOrDefaultAsync();

        if (doctor == null)
        {
            return;
        }

        var service = new DoctorAvailabilityService(db);

        // Use a date far enough in the future where no roster
        // is expected to exist.
        var date = DateOnly.FromDateTime(
            DateTime.UtcNow.Date.AddDays(365));

        var slots = await service.GetAvailableSlotsAsync(
            doctor.Id,
            date,
            30);

        Assert.Empty(slots);
    }

    [Fact(Skip = "In-memory database doesn't support concurrent constraints")]
    public async Task ConcurrentBookings_ShouldNotBothSucceed()
    {
        // Get test doctor, patient and an available slot.
        await using var setupDb = CreateDbContext();

        var doctor = await setupDb.Doctors
            .FirstOrDefaultAsync();

        var patient = await setupDb.PatientProfiles
            .FirstOrDefaultAsync();

        if (doctor == null || patient == null)
        {
            return;
        }

        var availability =
            await setupDb.DoctorAvailabilities
                .FirstOrDefaultAsync(
                    a => a.DoctorId == doctor.Id);

        if (availability == null)
        {
            return;
        }

        var appointmentDate = availability.Date;
        var startTime = availability.StartTime;
        var endTime = startTime.AddMinutes(30);

        var dto1 = new CreateAppointmentDto
        {
            DoctorId = doctor.Id,
            PatientId = patient.Id,
            AppointmentDate = appointmentDate,
            StartTime = startTime,
            EndTime = endTime
        };

        var dto2 = new CreateAppointmentDto
        {
            DoctorId = doctor.Id,
            PatientId = patient.Id,
            AppointmentDate = appointmentDate,
            StartTime = startTime,
            EndTime = endTime
        };

        // Use separate DbContexts so both operations represent
        // independent database transactions.
        await using var db1 = CreateDbContext();
        await using var db2 = CreateDbContext();

        var service1 = new AppointmentService(db1);
        var service2 = new AppointmentService(db2);

        var task1 = service1.CreateTentativeAsync(dto1);
        var task2 = service2.CreateTentativeAsync(dto2);

        var results = await Task.WhenAll(
            CaptureResult(task1),
            CaptureResult(task2));

        var successfulBookings =
            results.Count(result => result.Success);

        // At most one booking for the same doctor/time
        // should succeed.
        Assert.True(
            successfulBookings <= 1,
            "Concurrent booking attempts should not both succeed.");
    }

    private static async Task<(bool Success, Exception? Error)>
        CaptureResult(Task task)
    {
        try
        {
            await task;

            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex);
        }
    }
}
