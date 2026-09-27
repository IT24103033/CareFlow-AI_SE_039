using System.Text.Json;
using Mscc.GenerativeAI;
using CareFlowAI.API.Models;

namespace CareFlowAI.API.Services
{
    /// <summary>
    /// Hybrid Planning Agent — combines instant rules-based critical detection
    /// with Google Gemini 2.0 Flash for nuanced clinical analysis.
    ///
    /// Strategy:
    ///   1. Scan symptoms for CRITICAL/HIGH keywords first (no network call — instant).
    ///      If matched → return immediately so the doctor sees it right away.
    ///   2. For non-critical symptoms → call Gemini for a full, nuanced clinical plan.
    ///   3. If Gemini fails (network error, quota) → fall back to rules so the system
    ///      never goes down.
    /// </summary>
    public class PlanningAgentService
    {
        private readonly IConfiguration _config;

        // ── Critical & High keyword rules (checked BEFORE calling Gemini) ─────
        // These cover emergency conditions where every second counts.
        private static readonly List<(string[] Keywords, string Specialist, string Urgency, string Action)> _criticalRules = new()
        {
            (new[] { "chest pain", "chest tightness", "heart attack", "palpitation" },
             "Cardiologist",  "Critical", "Send to Emergency Department immediately."),

            (new[] { "shortness of breath", "cannot breathe", "difficulty breathing" },
             "Pulmonologist", "Critical", "Send to Emergency Department immediately."),

            (new[] { "stroke", "face drooping", "sudden numbness", "arm weakness", "speech difficulty" },
             "Neurologist",   "Critical", "Send to Emergency Department immediately."),

            (new[] { "seizure", "unconscious", "unresponsive", "not breathing" },
             "Neurologist",   "Critical", "Send to Emergency Department immediately."),

            (new[] { "severe bleeding", "blood loss", "hemorrhage" },
             "General Surgeon","Critical", "Send to Emergency Department immediately."),

            (new[] { "severe headache", "migraine", "sudden head pain" },
             "Neurologist",   "High",     "Schedule urgent appointment within 24 hours."),

            (new[] { "burning urination", "blood in urine", "kidney pain" },
             "Urologist",     "High",     "Schedule urgent appointment within 24 hours."),
        };

        // ── Fallback rules (used only if Gemini API fails) ────────────────────
        private static readonly List<(string[] Keywords, string Specialist, string Urgency)> _fallbackRules = new()
        {
            (new[] { "abdominal pain", "stomach pain", "nausea", "vomiting", "diarrhea" },
             "Gastroenterologist", "Medium"),

            (new[] { "back pain", "joint pain", "muscle pain", "fracture", "sprain", "knee pain" },
             "Orthopedist", "Medium"),

            (new[] { "eye pain", "blurred vision", "redness in eye" },
             "Ophthalmologist", "Medium"),

            (new[] { "anxiety", "depression", "panic attack", "insomnia" },
             "Psychiatrist", "Medium"),

            (new[] { "rash", "itching", "skin irritation", "hives", "eczema" },
             "Dermatologist", "Low"),

            (new[] { "cough", "cold", "fever", "sore throat", "flu", "fatigue" },
             "General Practitioner", "Low"),
        };

        public PlanningAgentService(IConfiguration config)
        {
            _config = config;
        }

        // ── Public Entry Point ────────────────────────────────────────────────

        public async Task<AgentWorkflowState> RunAsync(TriageRecord record)
        {
            var state = new AgentWorkflowState
            {
                TriageRecordId = record.Id,
                AgentName      = "PlanningAgent",
                AgentStatus    = "Running",
                ApprovalStatus = "Pending",
                InputPayload   = JsonSerializer.Serialize(new
                {
                    PatientId = record.PatientId,
                    Symptoms  = record.Symptoms,
                    Severity  = record.SeverityLevel
                }),
                StartedAt = DateTime.UtcNow
            };

            try
            {
                var plan = await AnalyseAsync(record.Symptoms);
                state.OutputPayload = JsonSerializer.Serialize(plan);
                state.AgentStatus   = "Completed";
                state.CompletedAt   = DateTime.UtcNow;
                state.UpdatedAt     = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                state.AgentStatus  = "Failed";
                state.ErrorMessage = ex.Message;
                state.CompletedAt  = DateTime.UtcNow;
                state.UpdatedAt    = DateTime.UtcNow;
            }

            return state;
        }

        // ── Hybrid Analysis Logic ─────────────────────────────────────────────

        private async Task<ClinicalPlan> AnalyseAsync(string symptoms)
        {
            var lower = symptoms.ToLowerInvariant();

            // ── STEP 1: Critical/High keyword scan (instant, no API call) ─────
            // If the patient has an emergency keyword, we return immediately.
            // This ensures critical cases reach doctors without any delay.
            foreach (var (keywords, specialist, urgency, action) in _criticalRules)
            {
                var matched = keywords.Where(k => lower.Contains(k)).ToList();
                if (matched.Any())
                {
                    return new ClinicalPlan
                    {
                        SuggestedSpecialist = specialist,
                        UrgencyLevel        = urgency,
                        RecommendedAction   = action,
                        Rationale           = $"⚠️ CRITICAL KEYWORDS DETECTED by rule engine: {string.Join(", ", matched)}. " +
                                              $"Immediate escalation required — Gemini analysis bypassed for speed.",
                        AnalysisMethod      = "RuleEngine"
                    };
                }
            }

            // ── STEP 2: Gemini AI for nuanced, non-critical analysis ──────────
            // Non-emergency symptoms benefit from Gemini's broader medical knowledge.
            try
            {
                var plan = await CallGeminiAsync(symptoms);
                plan.AnalysisMethod = "GeminiAI";
                return plan;
            }
            catch
            {
                // ── STEP 3: Fallback rules if Gemini is unavailable ───────────
                // System must NEVER go down even if the Gemini API has an outage.
                return FallbackRules(lower);
            }
        }

        // ── Gemini API Call ───────────────────────────────────────────────────

        private async Task<ClinicalPlan> CallGeminiAsync(string symptoms)
        {
            var apiKey    = _config["Gemini:ApiKey"]!;
            var modelName = _config["Gemini:Model"] ?? "gemini-2.0-flash";

            var googleAI = new GoogleAI(apiKey);
            var model    = googleAI.GenerativeModel(modelName);

            var prompt = $$"""
                You are a clinical triage AI assistant for a hospital system.
                Analyse the following patient symptoms and return ONLY a valid JSON object.

                Patient symptoms: "{{symptoms}}"

                Return exactly this JSON structure (no markdown, no extra text):
                {
                  "SuggestedSpecialist": "<medical specialist type>",
                  "UrgencyLevel": "<one of: Critical, High, Medium, Low>",
                  "RecommendedAction": "<specific action for the doctor>",
                  "Rationale": "<brief clinical reasoning>"
                }
                """;

            var response = await model.GenerateContent(prompt);
            var raw      = response.Text ?? throw new Exception("Empty response from Gemini.");

            // Strip markdown code fences if Gemini adds them
            var json = raw
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();

            var plan = JsonSerializer.Deserialize<ClinicalPlan>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return plan ?? throw new Exception("Failed to parse Gemini response into ClinicalPlan.");
        }

        // ── Fallback Rules (Gemini unavailable) ───────────────────────────────

        private static ClinicalPlan FallbackRules(string lower)
        {
            foreach (var (keywords, specialist, urgency) in _fallbackRules)
            {
                if (keywords.Any(k => lower.Contains(k)))
                    return new ClinicalPlan
                    {
                        SuggestedSpecialist = specialist,
                        UrgencyLevel        = urgency,
                        RecommendedAction   = BuildAction(urgency),
                        Rationale           = "Analysed by fallback rule engine (Gemini unavailable).",
                        AnalysisMethod      = "FallbackRules"
                    };
            }

            return new ClinicalPlan
            {
                SuggestedSpecialist = "General Practitioner",
                UrgencyLevel        = "Low",
                RecommendedAction   = "Schedule a routine appointment for assessment.",
                Rationale           = "No specific indicators detected. Routine review recommended.",
                AnalysisMethod      = "FallbackRules"
            };
        }

        private static string BuildAction(string urgency) => urgency switch
        {
            "Critical" => "Send to Emergency Department immediately.",
            "High"     => "Schedule urgent appointment within 24 hours.",
            "Medium"   => "Schedule appointment within 3–5 days.",
            _          => "Schedule a routine appointment for assessment."
        };
    }

    // ── Output Model ──────────────────────────────────────────────────────────

    public class ClinicalPlan
    {
        public string SuggestedSpecialist { get; set; } = string.Empty;
        public string UrgencyLevel        { get; set; } = string.Empty;
        public string RecommendedAction   { get; set; } = string.Empty;
        public string Rationale           { get; set; } = string.Empty;
        /// <summary>Tracks which analysis path was used: RuleEngine | GeminiAI | FallbackRules</summary>
        public string AnalysisMethod      { get; set; } = string.Empty;
    }
}
