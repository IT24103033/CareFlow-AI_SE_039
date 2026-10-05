using System.Text.Json;
using CareFlowAI.Orchestrator.Abstractions;
using CareFlowAI.Orchestrator.Models;

namespace CareFlowAI.Orchestrator.Tools
{
    public class FindAvailableSlotsTool
    {
        private readonly IAvailabilityProvider _availabilityProvider;

        public FindAvailableSlotsTool(
            IAvailabilityProvider availabilityProvider)
        {
            _availabilityProvider = availabilityProvider;
        }

        // Allow-listed tool:
        // Finds available appointment slots for a doctor on a specific date.
        public async Task<AppointmentActionResult> ExecuteAsync(
            Guid doctorId,
            DateOnly date,
            int slotDurationMinutes = 30)
        {
            try
            {
                var slots = await _availabilityProvider.GetAvailableSlotsAsync(
                    doctorId,
                    date,
                    slotDurationMinutes);

                if (slots.Count == 0)
                {
                    return AppointmentActionResult.Unavailable(
                        "No available slots found for the selected doctor and date.");
                }

                return AppointmentActionResult.Success(
                    JsonSerializer.Serialize(slots));
            }
            catch (ArgumentException ex)
            {
                return AppointmentActionResult.InvalidRequest(
                    $"Invalid slot request: {ex.Message}");
            }
            catch (Exception)
            {
                return AppointmentActionResult.ProviderError(
                    "An unexpected error occurred while finding available slots.");
            }
        }
    }
}