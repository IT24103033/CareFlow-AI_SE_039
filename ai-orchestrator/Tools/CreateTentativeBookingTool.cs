using System.Net.Http.Json;

namespace CareFlowAI.Orchestrator.Tools
{
    public class CreateTentativeBookingTool
    {
        private readonly HttpClient _httpClient;

        public CreateTentativeBookingTool(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Allow-listed tool:
        // Creates a tentative appointment after
        // availability and conflict checks.
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
                    "http://localhost:5241/api/Appointments/tentative",
                    request);

                if (response.IsSuccessStatusCode)
                {
                    var result =
                        await response.Content.ReadAsStringAsync();

                    return result;
                }

                if ((int)response.StatusCode == 409)
                {
                    return "The selected appointment time is already booked.";
                }

                if ((int)response.StatusCode == 404)
                {
                    return "The selected doctor or patient could not be found.";
                }

                if ((int)response.StatusCode == 400)
                {
                    return "The appointment details are invalid.";
                }

                return "Unable to create the tentative appointment.";
            }
            catch (HttpRequestException)
            {
                return "Unable to connect to the CareFlow API.";
            }
            catch (Exception)
            {
                return "An unexpected error occurred while creating the tentative booking.";
            }
        }
    }
}