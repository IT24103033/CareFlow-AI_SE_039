using System.Text.Json;
using CareFlowAI.Orchestrator.Abstractions;
using CareFlowAI.Orchestrator.Models;

namespace CareFlowAI.Orchestrator.Tools
{
    public class CreateTentativeBookingTool
    {
        private readonly IAppointmentBookingProvider _bookingProvider;

        public CreateTentativeBookingTool(
            IAppointmentBookingProvider bookingProvider)
        {
            _bookingProvider = bookingProvider;
        }

        // Allow-listed tool:
        // Creates a tentative appointment after
        // availability and conflict checks.
        public async Task<AppointmentActionResult> ExecuteAsync(
            Guid doctorId,
            Guid patientId,
            DateOnly appointmentDate,
            TimeOnly startTime,
            TimeOnly endTime)
        {
            try
            {
                var bookingResult =
                    await _bookingProvider.CreateTentativeAsync(
                        doctorId,
                        patientId,
                        appointmentDate,
                        startTime,
                        endTime);

                // The provider currently returns the appointment ID
                // as a string. Preserve that durable identifier.
                if (Guid.TryParse(bookingResult, out var appointmentId))
                {
                    return AppointmentActionResult.Success(
                        "Tentative appointment created successfully.",
                        appointmentId);
                }

                // If the provider returns a JSON object containing
                // an appointment ID, try to extract it.
                try
                {
                    using var document =
                        JsonDocument.Parse(bookingResult);

                    if (document.RootElement.TryGetProperty(
                            "appointmentId",
                            out var appointmentIdElement) &&
                        Guid.TryParse(
                            appointmentIdElement.GetString(),
                            out var parsedAppointmentId))
                    {
                        return AppointmentActionResult.Success(
                            "Tentative appointment created successfully.",
                            parsedAppointmentId);
                    }
                }
                catch (JsonException)
                {
                    // The provider returned a non-JSON result.
                }

                // Keep the provider response as the message if it
                // does not contain a recognizable appointment ID.
                if (!string.IsNullOrWhiteSpace(bookingResult))
                {
                    return AppointmentActionResult.Success(
                        bookingResult);
                }

                return AppointmentActionResult.ProviderError(
                    "The tentative booking was created, but no appointment identifier was returned.");
            }
            catch (ArgumentException ex)
            {
                return AppointmentActionResult.InvalidRequest(
                    $"Invalid booking request: {ex.Message}");
            }
            catch (Exception)
            {
                return AppointmentActionResult.ProviderError(
                    "An unexpected error occurred while creating the tentative booking.");
            }
        }
    }
}