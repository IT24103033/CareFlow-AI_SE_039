using System.Net.Http.Json;

namespace CareFlowAI.Orchestrator.Tools
{
    public class CheckBookingConflictTool
    {
        private readonly HttpClient _httpClient;

        public CheckBookingConflictTool(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Allow-listed tool:
        // Checks whether an appointment time conflicts
        // with an existing appointment.
        public async Task<string> ExecuteAsync(
            Guid doctorId,
            Guid patientId,
            DateOnly appointmentDate,
            TimeOnly startTime,
            TimeOnly endTime)
        {
            try
            {
                var request = new
                {
                    doctorId = doctorId,
                    patientId = patientId,
                    appointmentDate = appointmentDate,
                    startTime = startTime,
                    endTime = endTime
                };

                var response = await _httpClient.PostAsJsonAsync(
                    "http://localhost:5241/api/Appointments/check-conflict",
                    request);

                if (!response.IsSuccessStatusCode)
                {
                    return "Unable to check booking conflict.";
                }

                var result =
                    await response.Content.ReadAsStringAsync();

                return result;
            }
            catch (HttpRequestException)
            {
                return "Unable to connect to the CareFlow API.";
            }
            catch (Exception)
            {
                return "An unexpected error occurred while checking the booking conflict.";
            }
        }
    }
}