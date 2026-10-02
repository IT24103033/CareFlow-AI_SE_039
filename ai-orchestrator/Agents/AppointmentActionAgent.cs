using System.Text.Json;
using CareFlowAI.Orchestrator.Models;
using CareFlowAI.Orchestrator.Tools;

namespace CareFlowAI.Orchestrator.Agents
{
    public class AppointmentActionAgent
    {
        private readonly FindAvailableSlotsTool _findAvailableSlotsTool;
        private readonly CheckBookingConflictTool _checkBookingConflictTool;
        private readonly CreateTentativeBookingTool _createTentativeBookingTool;

        public AppointmentActionAgent(
            FindAvailableSlotsTool findAvailableSlotsTool,
            CheckBookingConflictTool checkBookingConflictTool,
            CreateTentativeBookingTool createTentativeBookingTool)
        {
            _findAvailableSlotsTool = findAvailableSlotsTool;
            _checkBookingConflictTool = checkBookingConflictTool;
            _createTentativeBookingTool = createTentativeBookingTool;
        }

        // Appointment Action Agent workflow
        //
        // 1. Find available slots
        // 2. Verify the requested slot exists
        // 3. Check booking conflict
        // 4. Create tentative booking
        //
        // The agent does NOT directly access the database.
        // It can only use the explicitly allow-listed tools.
        public async Task<AppointmentActionResult> FindAndBookAsync(
            Guid doctorId,
            Guid patientId,
            DateOnly appointmentDate,
            TimeOnly startTime,
            TimeOnly endTime)
        {
            // Step 1: Find available slots
            var availableSlotsResult =
                await _findAvailableSlotsTool.ExecuteAsync(
                    doctorId,
                    appointmentDate);

            // Pass structured failures directly to the caller.
            if (availableSlotsResult.Status != AppointmentActionStatus.Success)
            {
                return availableSlotsResult;
            }

            // Step 2: Verify that the exact requested slot
            // exists in the doctor's available slots.
            try
            {
                using var document =
                    JsonDocument.Parse(availableSlotsResult.Message);

                var requestedSlotExists =
                    document.RootElement.EnumerateArray()
                        .Any(slot =>
                            slot.GetProperty("startTime")
                                .GetString() == startTime.ToString("HH:mm:ss")
                            &&
                            slot.GetProperty("endTime")
                                .GetString() == endTime.ToString("HH:mm:ss"));

                if (!requestedSlotExists)
                {
                    return AppointmentActionResult.Unavailable(
                        "The requested appointment time is not available.");
                }
            }
            catch (JsonException)
            {
                return AppointmentActionResult.ProviderError(
                    "Unable to process the available appointment slots.");
            }

            // Step 3: Check for an existing booking conflict
            var conflictResult =
                await _checkBookingConflictTool.ExecuteAsync(
                    doctorId,
                    patientId,
                    appointmentDate,
                    startTime,
                    endTime);

            if (conflictResult.Status != AppointmentActionStatus.Success)
            {
                return conflictResult;
            }

            // Step 4: Create a tentative booking
            var bookingResult =
                await _createTentativeBookingTool.ExecuteAsync(
                    doctorId,
                    patientId,
                    appointmentDate,
                    startTime,
                    endTime);

            return bookingResult;
        }
    }
}