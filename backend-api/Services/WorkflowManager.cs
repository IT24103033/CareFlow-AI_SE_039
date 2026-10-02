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
            var appointmentAgent = scope.ServiceProvider.GetRequiredService<CareFlowAI.Orchestrator.Agents.AppointmentActionAgent>();

            var threshold = DateTime.UtcNow.AddMinutes(-5);

            var abandonedIds = await context.AgentWorkflows
                .Where(w => w.AgentName == "PlanningAgent" && w.AgentStatus == "Running" && w.StartedAt < threshold)
                .Select(w => w.Id)
                .ToListAsync(stoppingToken);

            foreach (var id in abandonedIds)
            {
                if (stoppingToken.IsCancellationRequested) break;

                using var transaction = await context.Database.BeginTransactionAsync(stoppingToken);

                // Atomically claim the state
                var claimed = await context.AgentWorkflows
                    .Where(w => w.Id == id && w.AgentStatus == "Running")
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(w => w.AgentStatus, "Abandoned")
                        .SetProperty(w => w.ErrorMessage, "Abandoned by orchestrator")
                        .SetProperty(w => w.UpdatedAt, DateTime.UtcNow), stoppingToken);
                
                if (claimed == 0) continue; // Another worker got it

                var oldState = await context.AgentWorkflows.Include(w => w.TriageRecord).SingleAsync(w => w.Id == id);
                var record = oldState.TriageRecord;
                if (record == null) continue;

                _logger.LogWarning($"Recovering abandoned PlanningAgent workflow {id} for TriageRecord {record.Id}");

                // Read old duration from input payload if possible
                string? duration = null;
                try {
                    var inputPayload = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.Nodes.JsonObject>(oldState.InputPayload);
                    duration = inputPayload?["Duration"]?.ToString();
                } catch { /* ignore */ }

                var newState = PlanningAgentService.CreateRunningState(record, duration);
                context.AgentWorkflows.Add(newState);
                await context.SaveChangesAsync(stoppingToken);
                await transaction.CommitAsync(stoppingToken);

                // Re-run the planning agent
                await planningAgent.RunAsync(record, newState, stoppingToken);

                var plan = newState.AgentStatus == "Completed" ? TriageReviewRules.ReadPlan(newState.OutputPayload) : null;
                
                if (record.TriageStatus == "Pending" || record.TriageStatus == "ReassessmentInProgress")
                {
                    record.TriageStatus = plan == null ? "AssessmentFailed" : "InReview";
                    record.SeverityLevel = plan?.UrgencyLevel ?? "Unassessed";
                    
                    if (plan != null)
                    {
                        await TriageOrchestrationHelper.ExecuteDownstreamActionsAsync(
                            record, plan, context, safetyAgent, appointmentAgent);
                    }
                    record.UpdatedAt = DateTime.UtcNow;
                }

                newState.UpdatedAt = DateTime.UtcNow;
                await context.SaveChangesAsync(stoppingToken);
                
                _logger.LogInformation($"Successfully recovered workflow via new state {newState.Id}. Status: {newState.AgentStatus}");
            }
        }
    }
}