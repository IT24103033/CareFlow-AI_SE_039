using CareFlowAI.API.Data;
using CareFlowAI.API.DTOs;
using CareFlowAI.API.Models;
using CareFlowAI.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CareFlowAI.API.Tests;

public class AppointmentSchedulingPostgreSqlTests
{
    private sealed record TestScenario(
        Guid DoctorId,
        Guid PatientId,
        Guid AvailabilityId,
        DateOnly Date,
        TimeOnly StartTime,
        TimeOnly EndTime);

    private static string GetConnectionString()
    {
        // First allow an environment variable to override configuration.
        // This is useful for local testing and CI without committing
        // database credentials to the repository.
        var environmentConnection =
            Environment.GetEnvironmentVariable(
                "ConnectionStrings__DefaultConnection");

        if (!string.IsNullOrWhiteSpace(environmentConnection))
        {
            return environmentConnection;
        }

        // Locate the solution root.
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory != null &&
               !File.Exists(
                   Path.Combine(
                       directory.FullName,
                       "CareFlow-AI_SE_039.sln")))
        {
            directory = directory.Parent;
        }

        if (directory == null)
        {
            throw new InvalidOperationException(
                "Could not locate the CareFlow-AI_SE_039 solution root.");
        }

        // Load the normal configuration first and then the Development
        // configuration. The Development file is ignored by Git and
        // contains the local Neon connection configuration.
        var configuration = new ConfigurationBuilder()
            .SetBasePath(directory.FullName)
            .AddJsonFile(
                "backend-api/appsettings.json",
                optional: false,
                reloadOnChange: false)
            .AddJsonFile(
                "backend-api/appsettings.Development.json",
                optional: true,
                reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        var connectionString =
            configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "No DefaultConnection was found for PostgreSQL integration tests.");
        }

        if (connectionString.Contains(
                "YOUR_",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "The PostgreSQL connection string still contains a placeholder password.");
        }

        return connectionString;
    }

    private static ApplicationDbContext CreatePostgreSqlDbContext()
    {
        var options =
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(GetConnectionString())
                .Options;

        return new ApplicationDbContext(options);
    }

    private static async Task<TestScenario> CreateScenarioAsync(
        ApplicationDbContext db)
    {
        var doctorId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var availabilityId = Guid.NewGuid();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var doctor = new Doctor
        {
            Id = doctorId,
            FullName = $"SE3110 Test Doctor {doctorId:N}",
            Specialization = "Integration Testing",
            Email = $"se3110-{doctorId:N}@test.local",
            IsActive = true
        };

        var patient = new PatientProfile
        {
            Id = patientId,
            FullName = $"SE3110 Test Patient {patientId:N}",
            DateOfBirth = new DateOnly(1990, 1, 1),
            BloodGroup = "O+",
            MedicalHistorySummary =
                "SE3110 PostgreSQL integration test patient",
            CreatedAt = DateTime.UtcNow
        };

        var availability = new DoctorAvailability
        {
            Id = availabilityId,
            DoctorId = doctorId,
            Date = today.AddDays(2),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(17, 0)
        };

        db.Doctors.Add(doctor);
        db.PatientProfiles.Add(patient);
        db.DoctorAvailabilities.Add(availability);

        await db.SaveChangesAsync();

        return new TestScenario(
            doctorId,
            patientId,
            availabilityId,
            availability.Date,
            availability.StartTime,
            availability.EndTime);
    }

    private static async Task CleanupScenarioAsync(
        Guid doctorId,
        Guid patientId)
    {
        await using var db = CreatePostgreSqlDbContext();

        // Remove appointments created by this test first because they
        // reference the doctor and patient.
        var appointments = await db.Appointments
            .Where(a =>
                a.DoctorId == doctorId ||
                a.PatientId == patientId)
            .ToListAsync();

        if (appointments.Count > 0)
        {
            db.Appointments.RemoveRange(appointments);
        }

        // Remove doctor availability records created by this test.
        var availabilities = await db.DoctorAvailabilities
            .Where(a => a.DoctorId == doctorId)
            .ToListAsync();

        if (availabilities.Count > 0)
        {
            db.DoctorAvailabilities.RemoveRange(availabilities);
        }

        // Remove test patient.
        var patient = await db.PatientProfiles
            .FirstOrDefaultAsync(p => p.Id == patientId);

        if (patient != null)
        {
            db.PatientProfiles.Remove(patient);
        }

        // Remove test doctor.
        var doctor = await db.Doctors
            .FirstOrDefaultAsync(d => d.Id == doctorId);

        if (doctor != null)
        {
            db.Doctors.Remove(doctor);
        }

        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task NeonPostgreSql_ShouldBeReachable()
    {
        await using var db = CreatePostgreSqlDbContext();

        var canConnect = await db.Database.CanConnectAsync();

        Assert.True(
            canConnect,
            "The configured Neon PostgreSQL database could not be reached.");
    }

    [Fact]
    public async Task Neon_ShouldReturnAvailableDoctorSlots()
    {
        await using var db = CreatePostgreSqlDbContext();

        var scenario = await CreateScenarioAsync(db);

        try
        {
            var service = new DoctorAvailabilityService(db);

            var slots = await service.GetAvailableSlotsAsync(
                scenario.DoctorId,
                scenario.Date,
                30);

            Assert.NotNull(slots);

            // 09:00 to 17:00 gives sixteen 30-minute slots.
            Assert.Equal(16, slots.Count);
        }
        finally
        {
            await CleanupScenarioAsync(
                scenario.DoctorId,
                scenario.PatientId);
        }
    }

    [Fact]
    public async Task Neon_ShouldReturnEmptySlots_WhenNoAvailabilityExists()
    {
        await using var db = CreatePostgreSqlDbContext();

        var scenario = await CreateScenarioAsync(db);

        try
        {
            var service = new DoctorAvailabilityService(db);

            var dateWithoutAvailability =
                scenario.Date.AddDays(100);

            var slots = await service.GetAvailableSlotsAsync(
                scenario.DoctorId,
                dateWithoutAvailability,
                30);

            Assert.NotNull(slots);
            Assert.Empty(slots);
        }
        finally
        {
            await CleanupScenarioAsync(
                scenario.DoctorId,
                scenario.PatientId);
        }
    }

    [Fact]
    public async Task Neon_ShouldDetectExistingAppointmentConflict()
    {
        await using var db = CreatePostgreSqlDbContext();

        var scenario = await CreateScenarioAsync(db);

        try
        {
            var existingAppointment = new Appointment
            {
                Id = Guid.NewGuid(),
                DoctorId = scenario.DoctorId,
                PatientId = scenario.PatientId,
                AppointmentDate = scenario.Date,
                StartTime = new TimeOnly(10, 0),
                EndTime = new TimeOnly(10, 30),
                Status = "Tentative",
                ApprovalStatus = "Pending",
                ApprovedByDoctorId = null,
                ApprovedAt = null,
                CreatedAt = DateTime.UtcNow
            };

            db.Appointments.Add(existingAppointment);

            await db.SaveChangesAsync();

            var service = new AppointmentService(db);

            var conflict = await service.CheckConflictAsync(
                scenario.DoctorId,
                scenario.Date,
                new TimeOnly(10, 0),
                new TimeOnly(10, 30));

            Assert.True(
                conflict,
                "An overlapping appointment should be detected as a conflict.");
        }
        finally
        {
            await CleanupScenarioAsync(
                scenario.DoctorId,
                scenario.PatientId);
        }
    }

    [Fact]
    public async Task Neon_ShouldPersistAppointmentApprovalAndConfirmation()
    {
        await using var db = CreatePostgreSqlDbContext();

        var scenario = await CreateScenarioAsync(db);

        try
        {
            var service = new AppointmentService(db);

            var createRequest = new CreateAppointmentDto
            {
                DoctorId = scenario.DoctorId,
                PatientId = scenario.PatientId,
                AppointmentDate = scenario.Date,
                StartTime = new TimeOnly(11, 0),
                EndTime = new TimeOnly(11, 30)
            };

            var created = await service.CreateTentativeAsync(
                createRequest);

            Assert.NotNull(created);

            Assert.Equal(
                "Tentative",
                created.Status);

            Assert.Equal(
                "Pending",
                created.ApprovalStatus);

            var approved = await service.ApproveAsync(
                created.Id,
                scenario.DoctorId);

            Assert.NotNull(approved);

            Assert.Equal(
                "Approved",
                approved!.ApprovalStatus);

            Assert.Equal(
                scenario.DoctorId,
                approved.ApprovedByDoctorId);

            Assert.NotNull(approved.ApprovedAt);

            var confirmed = await service.ConfirmAsync(
                created.Id);

            Assert.NotNull(confirmed);

            Assert.Equal(
                "Confirmed",
                confirmed!.Status);

            Assert.Equal(
                "Approved",
                confirmed.ApprovalStatus);

            // Verify the final state using a completely new DbContext.
            await using var verificationDb =
                CreatePostgreSqlDbContext();

            var persisted = await verificationDb.Appointments
                .FirstOrDefaultAsync(a => a.Id == created.Id);

            Assert.NotNull(persisted);

            Assert.Equal(
                "Confirmed",
                persisted!.Status);

            Assert.Equal(
                "Approved",
                persisted.ApprovalStatus);

            Assert.Equal(
                scenario.DoctorId,
                persisted.ApprovedByDoctorId);

            Assert.NotNull(persisted.ApprovedAt);
        }
        finally
        {
            await CleanupScenarioAsync(
                scenario.DoctorId,
                scenario.PatientId);
        }
    }

    [Fact]
    public async Task Neon_ConcurrentBookings_ShouldNotBothSucceed()
    {
        await using var setupDb = CreatePostgreSqlDbContext();

        var scenario = await CreateScenarioAsync(setupDb);

        try
        {
            var slotStart = new TimeOnly(14, 0);
            var slotEnd = new TimeOnly(14, 30);

            var dto1 = new CreateAppointmentDto
            {
                DoctorId = scenario.DoctorId,
                PatientId = scenario.PatientId,
                AppointmentDate = scenario.Date,
                StartTime = slotStart,
                EndTime = slotEnd
            };

            var dto2 = new CreateAppointmentDto
            {
                DoctorId = scenario.DoctorId,
                PatientId = scenario.PatientId,
                AppointmentDate = scenario.Date,
                StartTime = slotStart,
                EndTime = slotEnd
            };

            // Separate DbContexts represent two independent requests.
            await using var db1 = CreatePostgreSqlDbContext();
            await using var db2 = CreatePostgreSqlDbContext();

            var service1 = new AppointmentService(db1);
            var service2 = new AppointmentService(db2);

            var task1 = CaptureBookingResultAsync(
                service1,
                dto1);

            var task2 = CaptureBookingResultAsync(
                service2,
                dto2);

            var results = await Task.WhenAll(
                task1,
                task2);

            var successfulBookings =
                results.Count(result => result.Success);

            // Under PostgreSQL SERIALIZABLE isolation, at most one
            // concurrent request should successfully create the booking.
            Assert.Equal(
                1,
                successfulBookings);

            var failedBookings =
                results
                    .Where(result => !result.Success)
                    .ToList();

            Assert.Single(failedBookings);

            Assert.IsType<InvalidOperationException>(
                failedBookings[0].Error);

            // Verify only one appointment actually exists.
            await using var verificationDb =
                CreatePostgreSqlDbContext();

            var appointments = await verificationDb.Appointments
                .Where(a =>
                    a.DoctorId == scenario.DoctorId &&
                    a.PatientId == scenario.PatientId &&
                    a.AppointmentDate == scenario.Date &&
                    a.StartTime == slotStart &&
                    a.EndTime == slotEnd &&
                    a.Status != "Cancelled")
                .ToListAsync();

            Assert.Single(appointments);
        }
        finally
        {
            await CleanupScenarioAsync(
                scenario.DoctorId,
                scenario.PatientId);
        }
    }

    private static async Task<(bool Success, Exception? Error)>
        CaptureBookingResultAsync(
            AppointmentService service,
            CreateAppointmentDto dto)
    {
        try
        {
            await service.CreateTentativeAsync(dto);

            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex);
        }
    }
}