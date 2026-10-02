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
        public string PatientHistoryUsed { get; set; } = string.Empty;
    }

    public class DomainAnalysisAgent
    {
        private readonly IPatientHistoryTool _historyTool;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;

        public DomainAnalysisAgent(string apiKey, IPatientHistoryTool historyTool, string? model = null)
        {
            _historyTool = historyTool ?? new FetchPatientHistory();
            _httpClient = new HttpClient();
            _apiKey = apiKey;
            _model = string.IsNullOrWhiteSpace(model) ? "gemini-flash-latest" : model;
        }

        public async Task<AgentOutput> AnalyzeRiskAsync(AgentInput input)
        {
            string medicalHistory = await _historyTool.ExecuteAsync(input.PatientName);

            string systemPrompt = $@"
You are a medical domain analysis AI for hospital ward triage.

Patient name: {input.PatientName}
Current symptoms: {input.CurrentSymptoms}
Stored patient history from the hospital record:
{medicalHistory}

Rules:
- Base the recommendation on BOTH current symptoms AND the stored history.
- flaggedFactors must cite real items from symptoms and/or history (conditions, allergies, blood group constraints, chronic disease).
- Do not invent symptoms the patient did not report (for example do not invent fever or shortness of breath).
- If history is missing, include that as a flagged factor and rely on symptoms only.
- Return strictly valid JSON matching this schema. Do not use markdown.
{{
    ""riskLevel"": ""High|Medium|Low"",
    ""flaggedFactors"": [""reason 1"", ""reason 2""],
    ""recommendedWardType"": ""ICU|General""
}}";

            var requestBody = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = systemPrompt } } }
                },
                generationConfig = new
                {
                    responseMimeType = "application/json"
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            try
            {
                if (string.IsNullOrWhiteSpace(_apiKey) || _apiKey.Contains("YOUR_GEMINI_API_KEY"))
                {
                    throw new InvalidOperationException("Gemini API key is not configured. Set Gemini:ApiKey in appsettings.Development.json.");
                }

                if (_apiKey.StartsWith("sk-", StringComparison.Ordinal))
                {
                    throw new InvalidOperationException("GeminiApiKey is an OpenAI key (sk-...). This agent calls Google Gemini, which needs a Google AI Studio key.");
                }

                using var request = new HttpRequestMessage(HttpMethod.Post, $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent");
                request.Headers.TryAddWithoutValidation("x-goog-api-key", _apiKey);
                request.Content = content;

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Gemini API error ({(int)response.StatusCode} {response.ReasonPhrase}): {errorBody}");
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var jsonDoc = JsonDocument.Parse(responseString);

                var llmResponse = jsonDoc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text").GetString();

                var parsed = ValidateAndParseOutput(llmResponse ?? "{}");
                parsed.PatientHistoryUsed = medicalHistory;
                return parsed;
            }
            catch (Exception ex)
            {
                return new AgentOutput
                {
                    RiskLevel = "Unknown",
                    FlaggedFactors = new[] { "Cloud AI Connection Failed.", ex.Message },
                    RecommendedWardType = "Unavailable",
                    PatientHistoryUsed = medicalHistory
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
                    RiskLevel = "Unknown",
                    FlaggedFactors = new[] { "AI Parsing Failed.", ex.Message },
                    RecommendedWardType = "Unavailable"
                };
            }
        }
    }
}
