using System.Net.Http.Json;

namespace CareFlowAI.Orchestrator.Tools
{
    public class FindAvailableSlotsTool
    {
        private readonly HttpClient _httpClient;

        public FindAvailableSlotsTool(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Allow-listed tool:
        // Finds available appointment slots for a doctor on a specific date.
        public async Task<string> ExecuteAsync(
            Guid doctorId,
            DateOnly date,
            int slotDurationMinutes = 30)
        {
            try
            {
                var url =
                    $"http://localhost:5241/api/DoctorAvailability/slots" +
                    $"?doctorId={doctorId}" +
                    $"&date={date:yyyy-MM-dd}" +
                    $"&slotDurationMinutes={slotDurationMinutes}";

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    return "No available slots found for the selected doctor and date.";
                }

                var slots =
                    await response.Content.ReadAsStringAsync();

                return slots;
            }
            catch (HttpRequestException)
            {
                return "Unable to connect to the CareFlow API.";
            }
            catch (Exception)
            {
                return "An unexpected error occurred while finding available slots.";
            }
        }
    }
}