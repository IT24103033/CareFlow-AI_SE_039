using System.Net.Http;
using System.Threading.Tasks;

namespace CareFlowAI.Orchestrator.Tools
{
    public class FetchPatientHistory
    {
        private readonly HttpClient _httpClient;

        public FetchPatientHistory()
        {
            _httpClient = new HttpClient();
        }

        public async Task<string> ExecuteAsync(string patientName)
        {
            // The agent calls API endpoint directly
            var response = await _httpClient.GetAsync($"http://localhost:5241/api/PatientProfiles/search?name={patientName}");
            if (!response.IsSuccessStatusCode) return "Error: Patient not found.";
            
            return await response.Content.ReadAsStringAsync();
        }
    }
}