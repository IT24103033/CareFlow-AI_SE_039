using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CareFlowAI.API.Data;
using CareFlowAI.API.Services;
using CareFlowAI.API.Models;

namespace CareFlowAI.AIOrchestrator
{
    /// <summary>
    /// Background worker that recovers abandoned or failed AI workflows.
    /// This addresses "Order 2: Safe retry for failed assessments and recovery of abandoned Running workflows".
    /// </summary>
    public class WorkflowManager : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<WorkflowManager> _logger;

        public WorkflowManager(IServiceProvider services, ILogger<WorkflowManager> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("WorkflowManager background service is starting.");
            
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RecoverAbandonedWorkflows(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while recovering workflows.");
                }

                // Poll every 1 minute
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task RecoverAbandonedWorkflows(CancellationToken stoppingToken)
        {
            using var scope = _services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var planningAgent = scope.ServiceProvider.GetRequiredService<PlanningAgentService>();
            var safetyAgent = scope.ServiceProvider.GetRequiredService<ISafetyAgent>();

            // Workflows stuck in "Running" for more than 5 minutes are considered abandoned
            var threshold = DateTime.UtcNow.AddMinutes(-5);

            var abandonedWorkflows = await context.AgentWorkflows
                .Include(w => w.TriageRecord)
                .Where(w => w.AgentName == "PlanningAgent" && w.AgentStatus == "Running" && w.StartedAt < threshold)
                .ToListAsync(stoppingToken);

            foreach (var state in abandonedWorkflows)
            {
                if (stoppingToken.IsCancellationRequested) break;

                var record = state.TriageRecord;
                if (record == null) continue;

                _logger.LogWarning($"Recovering abandoned PlanningAgent workflow {state.Id} for TriageRecord {record.Id}");

                // Re-run the planning agent
                await planningAgent.RunAsync(record, state, stoppingToken);

                var plan = state.AgentStatus == "Completed" ? TriageReviewRules.ReadPlan(state.OutputPayload) : null;
                
                // Only update the TriageRecord status if it's currently Pending or ReassessmentInProgress
                if (record.TriageStatus == "Pending" || record.TriageStatus == "ReassessmentInProgress")
                {
                    record.TriageStatus = plan == null ? "AssessmentFailed" : "InReview";
                    record.SeverityLevel = plan?.UrgencyLevel ?? "Unassessed";
                    
                    if (plan != null)
                    {
                        var safetyWorkflowState = safetyAgent.CheckEmergencyRules(record, plan);
                        context.AgentWorkflows.Add(safetyWorkflowState);
                    }
                    record.UpdatedAt = DateTime.UtcNow;
                }

                state.UpdatedAt = DateTime.UtcNow;
                await context.SaveChangesAsync(stoppingToken);
                
                _logger.LogInformation($"Successfully recovered workflow {state.Id}. New status: {state.AgentStatus}");
            }
        }
    }
}