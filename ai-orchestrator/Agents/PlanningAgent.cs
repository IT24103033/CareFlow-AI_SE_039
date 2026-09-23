using System.Text.Json;
using CareFlowAI.API.Models;

namespace CareFlowAI.AIOrchestrator.Agents
{
    /// <summary>
    /// The Planning Agent — Component B's AI duty.
    /// Lives in ai-orchestrator/Agents/ as the team's AI agent layer.
    ///
    /// Analyses submitted symptoms using rules-based keyword matching
    /// and produces a structured clinical triage plan (ClinicalPlan).
    /// The plan is serialized as JSON and persisted in AgentWorkflowState.OutputPayload.
    /// </summary>
    public class PlanningAgent
    {
        // ── Specialist Keyword Rules ─────────────────────────────────────────
        // Maps symptom keywords → suggested specialist + urgency.
        // Ordered most critical first — first matching rule wins.
        private static readonly List<(string[] Keywords, string Specialist, string Urgency)> _rules = new()
        {
            (new[] { "chest pain", "chest tightness", "heart attack", "palpitation", "shortness of breath" },
             "Cardiologist", "Critical"),

            (new[] { "stroke", "sudden numbness", "face drooping", "arm weakness", "speech difficulty" },
             "Neurologist", "Critical"),

            (new[] { "severe headache", "migraine", "seizure", "confusion", "memory loss", "dizziness" },
             "Neurologist", "High"),

            (new[] { "abdominal pain", "stomach pain", "nausea", "vomiting", "diarrhea", "bloating" },
             "Gastroenterologist", "Medium"),

            (new[] { "burning urination", "urinary tract infection", "uti", "kidney pain", "blood in urine" },
             "Urologist", "High"),

            (new[] { "back pain", "joint pain", "muscle pain", "fracture", "sprain", "knee pain" },
             "Orthopedist", "Medium"),

            (new[] { "eye pain", "blurred vision", "redness in eye", "eye discharge" },
             "Ophthalmologist", "Medium"),

            (new[] { "anxiety", "depression", "panic attack", "mood swings", "insomnia", "stress" },
             "Psychiatrist", "Medium"),

            (new[] { "rash", "itching", "skin irritation", "hives", "eczema", "acne" },
             "Dermatologist", "Low"),

            (new[] { "cough", "cold", "fever", "sore throat", "runny nose", "flu", "fatigue" },
             "General Practitioner", "Low"),
        };

        // ── Public Entry Point ────────────────────────────────────────────────

        /// <summary>
        /// Runs the planning agent for a triage record.
        /// Returns a populated AgentWorkflowState ready to be saved to the DB.
        /// </summary>
        public AgentWorkflowState Run(TriageRecord record)
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
                var plan = Analyse(record.Symptoms);

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

        // ── Symptom Analysis Logic ────────────────────────────────────────────

        private static ClinicalPlan Analyse(string symptoms)
        {
            var lower = symptoms.ToLowerInvariant();

            foreach (var (keywords, specialist, urgency) in _rules)
            {
                if (keywords.Any(k => lower.Contains(k)))
                {
                    return new ClinicalPlan
                    {
                        SuggestedSpecialist = specialist,
                        UrgencyLevel        = urgency,
                        RecommendedAction   = BuildAction(urgency),
                        Rationale           = $"Symptoms match {specialist.ToLower()} indicators: " +
                                              string.Join(", ", keywords.Where(k => lower.Contains(k)))
                    };
                }
            }

            // Default — no high-acuity keywords matched
            return new ClinicalPlan
            {
                SuggestedSpecialist = "General Practitioner",
                UrgencyLevel        = "Low",
                RecommendedAction   = "Schedule a routine appointment for assessment.",
                Rationale           = "No high-acuity keywords detected in symptom description."
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

    /// <summary>
    /// The structured clinical plan produced by the Planning Agent.
    /// Serialized as JSON and stored in AgentWorkflowState.OutputPayload.
    /// </summary>
    public class ClinicalPlan
    {
        public string SuggestedSpecialist { get; set; } = string.Empty;
        public string UrgencyLevel        { get; set; } = string.Empty;
        public string RecommendedAction   { get; set; } = string.Empty;
        public string Rationale           { get; set; } = string.Empty;
    }
}
