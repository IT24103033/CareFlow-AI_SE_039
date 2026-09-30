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
        public string CurrentSymptoms { get; set; } = string.Empty;
    }

    public class AgentOutput
    {
        public string RiskLevel { get; set; } = string.Empty; 
        public string[] FlaggedFactors { get; set; } = Array.Empty<string>();
        public string RecommendedWardType { get; set; } = string.Empty; 
    }

    public interface IDomainAnalysisAgent
    {
        Task<AgentOutput> AnalyzeRiskAsync(AgentInput input, System.Threading.CancellationToken cancellationToken = default);
    }

    public class AgentPatientProfile
    {
        public string MedicalHistorySummary { get; set; } = string.Empty;
    }

    public interface IDomainContextAdapter
    {
        Task<AgentPatientProfile?> GetPatientProfileAsync(Guid patientId, System.Threading.CancellationToken cancellationToken);
    }

    public class DomainAnalysisAgent : IDomainAnalysisAgent
    {
        private readonly IDomainContextAdapter _contextAdapter;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public DomainAnalysisAgent(IDomainContextAdapter contextAdapter, string apiKey, HttpClient? httpClient = null)
        {
            _contextAdapter = contextAdapter;
            _httpClient = httpClient ?? new HttpClient();
            _apiKey = apiKey;
        }

        public async Task<AgentOutput> AnalyzeRiskAsync(AgentInput input, System.Threading.CancellationToken cancellationToken = default)
        {
            var profile = await _contextAdapter.GetPatientProfileAsync(input.PatientId, cancellationToken);
            string medicalHistory = profile?.MedicalHistorySummary ?? "Error: Patient not found.";

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
                    throw new DomainAnalysisException("DOMAIN_NOT_CONFIGURED", "Gemini API key is not configured.");
                }

                // Send the request over the internet
                string endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3.6-flash:generateContent?key={_apiKey}";
                var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
                
                if (!response.IsSuccessStatusCode)
                {
                    throw new DomainAnalysisException("DOMAIN_PROVIDER_UNAVAILABLE", $"Gemini API error ({(int)response.StatusCode} {response.ReasonPhrase})");
                }

                var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
                var jsonDoc = JsonDocument.Parse(responseString);
                
                // Navigate Gemini's response JSON tree to get the actual text
                var llmResponse = jsonDoc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text").GetString();

                return ValidateAndParseOutput(llmResponse ?? "{}");
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
                throw new DomainAnalysisException("DOMAIN_INVALID_OUTPUT", $"Failed to process provider response: {ex.GetType().Name}");
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
                throw new DomainAnalysisException("DOMAIN_INVALID_OUTPUT", "Validation Failed: AI returned malformed output.");
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