using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using CareFlowAI.API.Models;

namespace CareFlowAI.API.Services
{
    /// <summary>
    /// Component D – Validation / Safety Agent
    ///
    /// This is the AI agent responsible for cross-referencing a proposed prescription
    /// against system safety rules before pausing for a human doctor to approve.
    ///
    /// Checks performed:
    ///   1. Drug-drug interaction warnings (hardcoded rule set – easy to swap for LLM call)
    ///   2. Emergency flag: Critical triage severity + any medicine with stock = 0
    ///   3. Low-stock alert: ordered quantity exceeds current stock
    ///   4. Expired medicine detection
    ///   5. Category-based severity contraindications (e.g. high-risk drugs on "Low" triage)
    ///
    /// Returns: SafetyResult with Verdict ("Safe" | "Warning" | "Blocked") + details JSON.
    /// </summary>
    public class PharmacyAiService
    {
        // ── Hardcoded Drug Interaction Rule Set ──────────────────────────────
        // Key: sorted pair "DrugA|DrugB" (lowercase, case-insensitive)
        // Value: human-readable interaction warning
        private static readonly Dictionary<string, string> InteractionRules = new(StringComparer.OrdinalIgnoreCase)
        {
            ["amoxicillin|warfarin"]           = "Amoxicillin can enhance the anticoagulant effect of Warfarin. Monitor INR closely.",
            ["ibuprofen|warfarin"]             = "NSAIDs like Ibuprofen increase bleeding risk when combined with Warfarin.",
            ["aspirin|warfarin"]               = "Aspirin + Warfarin significantly increases hemorrhage risk.",
            ["metformin|contrast"]             = "Metformin should be withheld before contrast imaging to avoid lactic acidosis.",
            ["ssri|tramadol"]                  = "SSRI + Tramadol can trigger serotonin syndrome. Consider alternative analgesic.",
            ["ciprofloxacin|antacid"]          = "Antacids reduce Ciprofloxacin absorption. Separate doses by 2 hours.",
            ["digoxin|amiodarone"]             = "Amiodarone increases Digoxin plasma levels – toxicity risk. Reduce Digoxin dose.",
            ["lisinopril|potassium"]           = "ACE inhibitors + Potassium supplements can cause dangerous hyperkalaemia.",
            ["simvastatin|amiodarone"]         = "Amiodarone can increase Simvastatin levels – risk of myopathy.",
            ["phenytoin|fluconazole"]          = "Fluconazole significantly raises Phenytoin levels – monitor for toxicity.",
        };

        // ── High-Risk Drug Categories (require Critical/High triage severity) ──
        private static readonly HashSet<string> HighRiskCategories = new(StringComparer.OrdinalIgnoreCase)
        {
            "Anticoagulant", "Chemotherapy", "Immunosuppressant", "Opioid", "Anaesthetic"
        };

        // ────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Runs all safety checks on the list of medicines in the prescription.
        /// </summary>
        /// <param name="items">PrescriptionItems with their resolved Medicine objects.</param>
        /// <param name="triageSeverity">Severity from the linked TriageRecord ("Low"|"Medium"|"High"|"Critical").</param>
        /// <returns>A SafetyResult containing the verdict and details.</returns>
        public SafetyResult RunSafetyCheck(IList<PrescriptionItem> items, string triageSeverity)
        {
            var warnings = new List<string>();
            var errors   = new List<string>();
            bool blocked = false;

            var medicines = items.Select(i => i.Medicine).ToList();
            var medicineNames = medicines.Select(m => m.Name).ToList();

            // ── Check 1: Drug-drug interactions ────────────────────────────────
            for (int i = 0; i < medicines.Count; i++)
            {
                for (int j = i + 1; j < medicines.Count; j++)
                {
                    var pairKey = BuildInteractionKey(medicines[i].Name, medicines[j].Name);
                    if (InteractionRules.TryGetValue(pairKey, out var warning))
                    {
                        warnings.Add($"⚠ Interaction [{medicines[i].Name} + {medicines[j].Name}]: {warning}");
                    }
                }
            }

            // ── Check 2: Stock vs ordered quantity ──────────────────────────────
            foreach (var item in items)
            {
                if (item.Medicine.StockQuantity < item.Quantity)
                {
                    if (item.Medicine.StockQuantity == 0)
                    {
                        errors.Add($"🚫 OUT OF STOCK: {item.Medicine.Name} has 0 units available. Ordered: {item.Quantity}.");
                        blocked = true;
                    }
                    else
                    {
                        warnings.Add($"⚠ LOW STOCK: {item.Medicine.Name} – only {item.Medicine.StockQuantity} units available, {item.Quantity} ordered.");
                    }
                }
            }

            // ── Check 3: Expired medicines ─────────────────────────────────────
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            foreach (var med in medicines)
            {
                if (med.ExpiryDate < today)
                {
                    errors.Add($"🚫 EXPIRED: {med.Name} expired on {med.ExpiryDate:yyyy-MM-dd}. Do not dispense.");
                    blocked = true;
                }
                else if (med.ExpiryDate < today.AddDays(30))
                {
                    warnings.Add($"⚠ NEAR EXPIRY: {med.Name} expires in {(med.ExpiryDate.ToDateTime(TimeOnly.MinValue) - DateTime.UtcNow).Days} days.");
                }
            }

            // ── Check 4: High-risk drug category vs triage severity ─────────────
            bool isLowSeverity = triageSeverity.Equals("Low", StringComparison.OrdinalIgnoreCase)
                              || triageSeverity.Equals("Medium", StringComparison.OrdinalIgnoreCase);
            foreach (var med in medicines)
            {
                if (HighRiskCategories.Contains(med.Category) && isLowSeverity)
                {
                    warnings.Add($"⚠ HIGH-RISK CATEGORY: {med.Name} ({med.Category}) prescribed for a '{triageSeverity}' severity case. Please verify clinical justification.");
                }
            }

            // ── Check 5: Critical triage + any out-of-stock item ────────────────
            bool isCritical = triageSeverity.Equals("Critical", StringComparison.OrdinalIgnoreCase);
            if (isCritical && items.Any(i => i.Medicine.StockQuantity == 0))
            {
                errors.Add("🚫 EMERGENCY: Patient triage is CRITICAL but required medication is out of stock. Immediate procurement required.");
                blocked = true;
            }

            // ── Determine overall verdict ────────────────────────────────────────
            string verdict;
            if (blocked || errors.Count > 0)
                verdict = "Blocked";
            else if (warnings.Count > 0)
                verdict = "Warning";
            else
                verdict = "Safe";

            // ── Build AI summary payload ─────────────────────────────────────────
            var resultPayload = new
            {
                AgentName     = "ValidationSafetyAgent",
                RunAt         = DateTime.UtcNow,
                TriageSeverity = triageSeverity,
                MedicinesChecked = medicineNames,
                Verdict       = verdict,
                Warnings      = warnings,
                Errors        = errors,
                Summary       = verdict == "Safe"
                    ? $"All {medicines.Count} medicine(s) passed safety checks. No interactions or stock issues detected."
                    : verdict == "Warning"
                        ? $"{warnings.Count} warning(s) found. Doctor review recommended before dispensing."
                        : $"{errors.Count} critical error(s) found. Prescription is BLOCKED until resolved."
            };

            return new SafetyResult
            {
                Verdict       = verdict,
                ResultJson    = JsonSerializer.Serialize(resultPayload, new JsonSerializerOptions { WriteIndented = true }),
                HasCriticalErrors = blocked
            };
        }

        // ── Helpers ──────────────────────────────────────────────────────────
        private static string BuildInteractionKey(string drug1, string drug2)
        {
            // Normalize: strip dosage info after first space, lowercase, sort alphabetically
            string a = drug1.Split(' ')[0].ToLowerInvariant();
            string b = drug2.Split(' ')[0].ToLowerInvariant();
            return string.Compare(a, b, StringComparison.Ordinal) < 0 ? $"{a}|{b}" : $"{b}|{a}";
        }
    }

    /// <summary>Result object returned by the ValidationSafetyAgent.</summary>
    public class SafetyResult
    {
        /// <summary>"Safe" | "Warning" | "Blocked"</summary>
        public string Verdict { get; set; } = "Safe";

        /// <summary>Full JSON payload of the agent's analysis.</summary>
        public string ResultJson { get; set; } = string.Empty;

        /// <summary>True when Verdict == "Blocked".</summary>
        public bool HasCriticalErrors { get; set; }
    }
}
