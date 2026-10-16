using System.Text.Json;
using CareFlowAI.Orchestrator.Abstractions;
using CareFlowAI.Orchestrator.Agents;
using CareFlowAI.Orchestrator.Models;
using CareFlowAI.Orchestrator.Tools;

namespace CareFlowAI.API.Tests;

public class AppointmentActionAgentTests
{
    private static readonly Guid DoctorId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly Guid PatientId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

    private static readonly DateOnly AppointmentDate =
        new(2026, 10, 15);

    private static readonly TimeOnly StartTime =
        new(10, 0);

    private static readonly TimeOnly EndTime =
        new(10, 30);

    private sealed class FakeAvailabilityProvider
        : IAvailabilityProvider
    {
        public List<AvailableSlotResult> Slots { get; set; } = new();

        public int CallCount { get; private set; }

        public bool ThrowArgumentException { get; set; }

        public bool ThrowGeneralException { get; set; }

        public Task<List<AvailableSlotResult>> GetAvailableSlotsAsync(
            Guid doctorId,
            DateOnly date,
            int slotDurationMinutes = 30)
        {
            CallCount++;

            if (ThrowArgumentException)
            {
                throw new ArgumentException(
                    "Invalid slot duration.");
            }

            if (ThrowGeneralException)
            {
                throw new InvalidOperationException(
                    "Availability provider unavailable.");
            }

            return Task.FromResult(Slots);
        }
    }

    private sealed class FakeAppointmentProvider
        : IAppointmentProvider
    {
        public bool HasConflict { get; set; }

        public int CallCount { get; private set; }

        public bool ThrowGeneralException { get; set; }

        public Task<bool> CheckConflictAsync(
            Guid doctorId,
            DateOnly appointmentDate,
            TimeOnly startTime,
            TimeOnly endTime)
        {
            CallCount++;

            if (ThrowGeneralException)
            {
                throw new InvalidOperationException(
                    "Appointment provider unavailable.");
            }

            return Task.FromResult(HasConflict);
        }
    }

    private sealed class FakeAppointmentBookingProvider
        : IAppointmentBookingProvider
    {
        public string BookingResult { get; set; } =
            Guid.NewGuid().ToString();

        public int CallCount { get; private set; }

        public bool ThrowGeneralException { get; set; }

        public Task<string> CreateTentativeAsync(
            Guid doctorId,
            Guid patientId,
            DateOnly appointmentDate,
            TimeOnly startTime,
            TimeOnly endTime)
        {
            CallCount++;

            if (ThrowGeneralException)
            {
                throw new InvalidOperationException(
                    "Booking provider unavailable.");
            }

            return Task.FromResult(BookingResult);
        }
    }

    private static (
        AppointmentActionAgent Agent,
        FakeAvailabilityProvider Availability,
        FakeAppointmentProvider Appointment,
        FakeAppointmentBookingProvider Booking)
        CreateAgent()
    {
        var availability =
            new FakeAvailabilityProvider();

        var appointment =
            new FakeAppointmentProvider();

        var booking =
            new FakeAppointmentBookingProvider();

        var findSlotsTool =
            new FindAvailableSlotsTool(
                availability);

        var conflictTool =
            new CheckBookingConflictTool(
                appointment);

        var bookingTool =
            new CreateTentativeBookingTool(
                booking);

        var agent =
            new AppointmentActionAgent(
                findSlotsTool,
                conflictTool,
                bookingTool);

        return (
            agent,
            availability,
            appointment,
            booking);
    }

    private static AvailableSlotResult CreateMatchingSlot()
    {
        return new AvailableSlotResult
        {
            DoctorId = DoctorId,
            DoctorName = "Test Doctor",
            Specialization = "General Medicine",
            Date = AppointmentDate,
            StartTime = StartTime,
            EndTime = EndTime
        };
    }

    [Fact]
    public async Task FindAndBookAsync_ShouldCreateTentativeBooking_WhenSlotIsAvailable()
    {
        var test = CreateAgent();

        test.Availability.Slots.Add(
            CreateMatchingSlot());

        var expectedAppointmentId = Guid.NewGuid();

        test.Booking.BookingResult =
            expectedAppointmentId.ToString();

        var result =
            await test.Agent.FindAndBookAsync(
                DoctorId,
                PatientId,
                AppointmentDate,
                StartTime,
                EndTime);

        Assert.Equal(
            AppointmentActionStatus.Success,
            result.Status);

        Assert.Equal(
            expectedAppointmentId,
            result.AppointmentId);

        Assert.Equal(
            1,
            test.Availability.CallCount);

        Assert.Equal(
            1,
            test.Appointment.CallCount);

        Assert.Equal(
            1,
            test.Booking.CallCount);
    }

    [Fact]
    public async Task FindAndBookAsync_ShouldReturnUnavailable_WhenNoSlotsExist()
    {
        var test = CreateAgent();

        var result =
            await test.Agent.FindAndBookAsync(
                DoctorId,
                PatientId,
                AppointmentDate,
                StartTime,
                EndTime);

        Assert.Equal(
            AppointmentActionStatus.Unavailable,
            result.Status);

        Assert.Contains(
            "No available slots",
            result.Message,
            StringComparison.OrdinalIgnoreCase);

        Assert.Equal(
            1,
            test.Availability.CallCount);

        Assert.Equal(
            0,
            test.Appointment.CallCount);

        Assert.Equal(
            0,
            test.Booking.CallCount);
    }

    [Fact]
    public async Task FindAndBookAsync_ShouldReturnUnavailable_WhenRequestedSlotDoesNotExist()
    {
        var test = CreateAgent();

        test.Availability.Slots.Add(
            new AvailableSlotResult
            {
                DoctorId = DoctorId,
                DoctorName = "Test Doctor",
                Specialization = "General Medicine",
                Date = AppointmentDate,
                StartTime = new TimeOnly(11, 0),
                EndTime = new TimeOnly(11, 30)
            });

        var result =
            await test.Agent.FindAndBookAsync(
                DoctorId,
                PatientId,
                AppointmentDate,
                StartTime,
                EndTime);

        Assert.Equal(
            AppointmentActionStatus.Unavailable,
            result.Status);

        Assert.Equal(
            "The requested appointment time is not available.",
            result.Message);

        // The agent must stop before conflict checking
        // because the requested slot was not available.
        Assert.Equal(
            0,
            test.Appointment.CallCount);

        Assert.Equal(
            0,
            test.Booking.CallCount);
    }

    [Fact]
    public async Task FindAndBookAsync_ShouldReturnConflict_WhenAppointmentAlreadyExists()
    {
        var test = CreateAgent();

        test.Availability.Slots.Add(
            CreateMatchingSlot());

        test.Appointment.HasConflict = true;

        var result =
            await test.Agent.FindAndBookAsync(
                DoctorId,
                PatientId,
                AppointmentDate,
                StartTime,
                EndTime);

        Assert.Equal(
            AppointmentActionStatus.Conflict,
            result.Status);

        Assert.Contains(
            "conflict",
            result.Message,
            StringComparison.OrdinalIgnoreCase);

        Assert.Equal(
            1,
            test.Availability.CallCount);

        Assert.Equal(
            1,
            test.Appointment.CallCount);

        // Booking must NOT happen after a conflict is detected.
        Assert.Equal(
            0,
            test.Booking.CallCount);
    }

    [Fact]
    public async Task FindAndBookAsync_ShouldReturnInvalidRequest_WhenAvailabilityToolRejectsRequest()
    {
        var test = CreateAgent();

        test.Availability.ThrowArgumentException = true;

        var result =
            await test.Agent.FindAndBookAsync(
                DoctorId,
                PatientId,
                AppointmentDate,
                StartTime,
                EndTime);

        Assert.Equal(
            AppointmentActionStatus.InvalidRequest,
            result.Status);

        Assert.Contains(
            "Invalid slot request",
            result.Message);

        Assert.Equal(
            0,
            test.Appointment.CallCount);

        Assert.Equal(
            0,
            test.Booking.CallCount);
    }

    [Fact]
    public async Task FindAndBookAsync_ShouldReturnProviderError_WhenAvailabilityProviderFails()
    {
        var test = CreateAgent();

        test.Availability.ThrowGeneralException = true;

        var result =
            await test.Agent.FindAndBookAsync(
                DoctorId,
                PatientId,
                AppointmentDate,
                StartTime,
                EndTime);

        Assert.Equal(
            AppointmentActionStatus.ProviderError,
            result.Status);

        Assert.Contains(
            "finding available slots",
            result.Message,
            StringComparison.OrdinalIgnoreCase);

        Assert.Equal(
            0,
            test.Appointment.CallCount);

        Assert.Equal(
            0,
            test.Booking.CallCount);
    }

    [Fact]
    public async Task FindAndBookAsync_ShouldReturnProviderError_WhenBookingProviderFails()
    {
        var test = CreateAgent();

        test.Availability.Slots.Add(
            CreateMatchingSlot());

        test.Booking.ThrowGeneralException = true;

        var result =
            await test.Agent.FindAndBookAsync(
                DoctorId,
                PatientId,
                AppointmentDate,
                StartTime,
                EndTime);

        Assert.Equal(
            AppointmentActionStatus.ProviderError,
            result.Status);

        Assert.Contains(
            "creating the tentative booking",
            result.Message,
            StringComparison.OrdinalIgnoreCase);

        Assert.Equal(
            1,
            test.Availability.CallCount);

        Assert.Equal(
            1,
            test.Appointment.CallCount);

        Assert.Equal(
            1,
            test.Booking.CallCount);
    }

    [Fact]
    public async Task FindAndBookAsync_ShouldExtractAppointmentId_FromProviderResponse()
    {
        var test = CreateAgent();

        test.Availability.Slots.Add(
            CreateMatchingSlot());

        var expectedAppointmentId = Guid.NewGuid();

        test.Booking.BookingResult =
            JsonSerializer.Serialize(
                new
                {
                    appointmentId =
                        expectedAppointmentId.ToString()
                });

        var result =
            await test.Agent.FindAndBookAsync(
                DoctorId,
                PatientId,
                AppointmentDate,
                StartTime,
                EndTime);

        Assert.Equal(
            AppointmentActionStatus.Success,
            result.Status);

        Assert.Equal(
            expectedAppointmentId,
            result.AppointmentId);

        Assert.Equal(
            1,
            test.Booking.CallCount);
    }

    [Fact]
    public async Task FindAndBookAsync_ShouldNotCreateBooking_WhenExactSlotHasDifferentDuration()
    {
        var test = CreateAgent();

        // Same start time but different end time.
        // The exact requested slot must exist.
        test.Availability.Slots.Add(
            new AvailableSlotResult
            {
                DoctorId = DoctorId,
                DoctorName = "Test Doctor",
                Specialization = "General Medicine",
                Date = AppointmentDate,
                StartTime = StartTime,
                EndTime = new TimeOnly(11, 0)
            });

        var result =
            await test.Agent.FindAndBookAsync(
                DoctorId,
                PatientId,
                AppointmentDate,
                StartTime,
                EndTime);

        Assert.Equal(
            AppointmentActionStatus.Unavailable,
            result.Status);

        Assert.Equal(
            0,
            test.Appointment.CallCount);

        Assert.Equal(
            0,
            test.Booking.CallCount);
    }
}