using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CareFlowAI.Orchestrator.Tools;

namespace CareFlowAI.Orchestrator.Agents
{
    public class AgentInput
    {
        public string PatientName { get; set; } = string.Empty;
        public string CurrentSymptoms { get; set; } = string.Empty;
    }

    public class AgentOutput
    {
        public string RiskLevel { get; set; } = string.Empty; 
        public string[] FlaggedFactors { get; set; } = Array.Empty<string>();
        public string RecommendedWardType { get; set; } = string.Empty; 
    }

    public class DomainAnalysisAgent
    {
        private readonly FetchPatientHistory _historyTool;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        // Constructor now requires the API key
        public DomainAnalysisAgent(string apiKey)
        {
            _historyTool = new FetchPatientHistory();
            _httpClient = new HttpClient();
            _apiKey = apiKey;
        }

        public async Task<AgentOutput> AnalyzeRiskAsync(AgentInput input)
        {
            string medicalHistory = await _historyTool.ExecuteAsync(input.PatientName);

            string systemPrompt = $@"
                You are a medical domain analysis AI.
                Current Symptoms: {input.CurrentSymptoms}
                Patient History: {medicalHistory}
                
                Analyze the risk and return strictly valid JSON matching this schema exactly. Do not use markdown blocks.
                {{
                    ""riskLevel"": ""High|Medium|Low"",
                    ""flaggedFactors"": [""reason 1"", ""reason 2""],
                    ""recommendedWardType"": ""ICU|General""
                }}";

            // Format the request for the Gemini API
            var requestBody = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = systemPrompt } } }
                },
                generationConfig = new 
                { 
                    responseMimeType = "application/json" // Force the AI to only output JSON
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            try
            {
                if (string.IsNullOrWhiteSpace(_apiKey) || _apiKey.Contains("YOUR_GEMINI_API_KEY"))
                {
                    throw new InvalidOperationException("Gemini API key is not configured. Please set 'GeminiApiKey' in appsettings.Development.json.");
                }

                // Send the request over the internet
                string endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3.6-flash:generateContent?key={_apiKey}";
                var response = await _httpClient.PostAsync(endpoint, content);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Gemini API error ({(int)response.StatusCode} {response.ReasonPhrase}): {errorBody}");
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var jsonDoc = JsonDocument.Parse(responseString);
                
                // Navigate Gemini's response JSON tree to get the actual text
                var llmResponse = jsonDoc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text").GetString();

                return ValidateAndParseOutput(llmResponse ?? "{}");
            }
            catch (Exception ex)
            {
                return new AgentOutput 
                { 
                    RiskLevel = "High", 
                    FlaggedFactors = new[] { "Cloud AI Connection Failed.", ex.Message }, 
                    RecommendedWardType = "ICU" 
                };
            }
        }

        private AgentOutput ValidateAndParseOutput(string llmResponse)
        {
            try
            {
                var result = JsonSerializer.Deserialize<AgentOutput>(llmResponse, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                if (result == null || string.IsNullOrEmpty(result.RiskLevel))
                    throw new Exception("Validation Failed: AI returned malformed output.");

                if (result.RiskLevel == "High" && result.RecommendedWardType != "ICU")
                    result.RecommendedWardType = "ICU"; 

                return result;
            }
            catch (Exception ex)
            {
                return new AgentOutput 
                { 
                    RiskLevel = "High", 
                    FlaggedFactors = new[] { "AI Parsing Failed.", ex.Message }, 
                    RecommendedWardType = "ICU" 
                };
            }
        }
    }
}