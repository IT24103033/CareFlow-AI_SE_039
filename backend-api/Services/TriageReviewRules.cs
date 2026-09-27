using System.Text.Json;
using CareFlowAI.API.Models;

namespace CareFlowAI.API.Services;

public static class TriageReviewRules
{
    public static ClinicalPlan? ReadPlan(string? payload)
    {
        try
        {
            var plan = JsonSerializer.Deserialize<ClinicalPlan>(payload ?? "null",
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return plan != null && PlanningPlanValidator.IsAssessmentValid(plan) &&
                (plan.SchemaVersion == 1 || PlanningPlanValidator.IsStructuredPlanValid(plan)) ? plan : null;
        }
        catch (JsonException) { return null; }
    }

    public static PlanningExecutionSummary? ReadExecution(string? payload)
    {
        try
        {
            return JsonSerializer.Deserialize<ClinicalPlan>(payload ?? "null",
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })?.Execution;
        }
        catch (JsonException) { return null; }
    }

    public static string? Check(TriageRecord record, AgentWorkflowState? agent, DateTime? expectedUpdatedAt)
    {
        if (expectedUpdatedAt != record.UpdatedAt)
            return "This case has changed. Refresh it before reviewing.";
        if (record.TriageStatus != "InReview")
            return "Only cases awaiting review can receive a decision.";
        if (agent?.AgentStatus != "Completed" || agent.ApprovalStatus != "Pending" || ReadPlan(agent.OutputPayload) is null)
            return "A completed, valid assessment awaiting approval is required.";
        return null;
    }
}
