using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using CareFlowAI.Orchestrator.Tools;

namespace CareFlowAI.Orchestrator.Agents
{
    public class DomainAnalysisException : Exception
    {
        public string ErrorCode { get; }
        public DomainAnalysisException(string errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }
    }

    public class AgentInput
    {
        public Guid PatientId { get; set; }
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

    public interface IDomainAnalysisAgent
    {
        Task<AgentOutput> AnalyzeRiskAsync(AgentInput input, System.Threading.CancellationToken cancellationToken = default);
    }

    public class DomainAnalysisAgent : IDomainAnalysisAgent
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

        public async Task<AgentOutput> AnalyzeRiskAsync(AgentInput input, System.Threading.CancellationToken cancellationToken = default)
        {
            string medicalHistory = "No history available.";
            try
            {
                medicalHistory = await _historyTool.ExecuteAsync(input.PatientName);
            }
            catch { /* fall through with default */ }

            string systemPrompt = $@"
You are a medical domain analysis AI for hospital ward triage.

Patient name: {input.PatientName}
Current symptoms: {input.CurrentSymptoms}
Stored patient history from the hospital record:
{medicalHistory}

Rules:
- Base the recommendation on BOTH current symptoms AND the stored history.
- flaggedFactors must cite real items from symptoms and/or history (conditions, allergies, blood group constraints, chronic disease).
- Do not invent symptoms the patient did not report.
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

                var response = await _httpClient.SendAsync(request, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new DomainAnalysisException("DOMAIN_PROVIDER_UNAVAILABLE", $"Gemini API error ({(int)response.StatusCode} {response.ReasonPhrase})");
                }

                var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
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
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (DomainAnalysisException)
            {
                throw;
            }
            catch (HttpRequestException ex)
            {
                throw new DomainAnalysisException("DOMAIN_PROVIDER_UNAVAILABLE", $"Network error communicating with provider: {ex.GetType().Name}");
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
            AgentOutput? result;
            try
            {
                result = JsonSerializer.Deserialize<AgentOutput>(llmResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (JsonException)
            {
                return new AgentOutput
                {
                    RiskLevel = "Unknown",
                    FlaggedFactors = new[] { "AI Parsing Failed." },
                    RecommendedWardType = "Unavailable"
                };
            }

            if (result == null)
                throw new DomainAnalysisException("DOMAIN_INVALID_OUTPUT", "Validation Failed: AI returned null output.");

            var validRisks = new[] { "Low", "Medium", "High" };
            if (string.IsNullOrEmpty(result.RiskLevel) || !Array.Exists(validRisks, r => r.Equals(result.RiskLevel, StringComparison.OrdinalIgnoreCase)))
                throw new DomainAnalysisException("DOMAIN_INVALID_OUTPUT", "Validation Failed: Invalid RiskLevel.");

            var validWards = new[] { "ICU", "General" };
            if (string.IsNullOrEmpty(result.RecommendedWardType) || !Array.Exists(validWards, w => w.Equals(result.RecommendedWardType, StringComparison.OrdinalIgnoreCase)))
                throw new DomainAnalysisException("DOMAIN_INVALID_OUTPUT", "Validation Failed: Invalid RecommendedWardType.");

            if (result.FlaggedFactors == null || result.FlaggedFactors.Any(f => string.IsNullOrWhiteSpace(f)))
                throw new DomainAnalysisException("DOMAIN_INVALID_OUTPUT", "Validation Failed: FlaggedFactors cannot be null or contain empty strings.");

            // Normalize casing
            result.RiskLevel = result.RiskLevel[..1].ToUpper() + result.RiskLevel[1..].ToLower();
            result.RecommendedWardType = result.RecommendedWardType.Equals("ICU", StringComparison.OrdinalIgnoreCase) ? "ICU" : "General";

            if (result.RiskLevel == "High" && result.RecommendedWardType != "ICU")
                result.RecommendedWardType = "ICU";

            return result;
        }
    }
}
