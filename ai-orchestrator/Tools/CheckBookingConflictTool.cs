using CareFlowAI.Orchestrator.Abstractions;
using CareFlowAI.Orchestrator.Models;

namespace CareFlowAI.Orchestrator.Tools
{
    public class CheckBookingConflictTool
    {
        private readonly IAppointmentProvider _appointmentProvider;

        public CheckBookingConflictTool(
            IAppointmentProvider appointmentProvider)
        {
            _appointmentProvider = appointmentProvider;
        }

        // Allow-listed tool:
        // Checks whether an appointment time conflicts
        // with an existing appointment.
        public async Task<AppointmentActionResult> ExecuteAsync(
            Guid doctorId,
            Guid patientId,
            DateOnly appointmentDate,
            TimeOnly startTime,
            TimeOnly endTime)
        {
            try
            {
                var hasConflict =
                    await _appointmentProvider.CheckConflictAsync(
                        doctorId,
                        appointmentDate,
                        startTime,
                        endTime);

                if (hasConflict)
                {
                    return AppointmentActionResult.Conflict(
                        "Booking conflict detected. The selected time is already booked.");
                }

                return AppointmentActionResult.Success(
                    "No booking conflict detected.");
            }
            catch (ArgumentException ex)
            {
                return AppointmentActionResult.InvalidRequest(
                    $"Invalid booking request: {ex.Message}");
            }
            catch (Exception)
            {
                return AppointmentActionResult.ProviderError(
                    "An unexpected error occurred while checking the booking conflict.");
            }
        }
    }
}