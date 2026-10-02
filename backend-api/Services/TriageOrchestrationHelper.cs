using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CareFlowAI.API.Data;
using CareFlowAI.API.Models;
using CareFlowAI.API.DTOs;
using CareFlowAI.Orchestrator.Agents;

namespace CareFlowAI.API.Services
{
    public static class TriageOrchestrationHelper
    {
        public static async Task ExecuteDownstreamActionsAsync(
            TriageRecord record,
            ClinicalPlan plan,
            ApplicationDbContext context,
            ISafetyAgent safetyAgent,
            AppointmentActionAgent appointmentAgent)
        {
            // 1. D Validation
            var safetyWorkflowState = safetyAgent.CheckEmergencyRules(record, plan);
            context.AgentWorkflows.Add(safetyWorkflowState);

            // 2. C Action (Tentative Booking)
            if (!string.IsNullOrWhiteSpace(plan.SuggestedSpecialist))
            {
                var doctor = await context.Doctors
                    .Where(d => d.Specialization == plan.SuggestedSpecialist && d.IsActive)
                    .FirstOrDefaultAsync();

                if (doctor != null)
                {
                    var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
                    var end = tomorrow.AddDays(7);
                    
                    var slot = await context.DoctorAvailabilities
                        .Where(a => a.DoctorId == doctor.Id && a.Date >= tomorrow && a.Date <= end)
                        .OrderBy(a => a.Date).ThenBy(a => a.StartTime)
                        .FirstOrDefaultAsync();
                        
                    if (slot != null)
                    {
                        var agentState = new AgentWorkflowState
                        {
                            TriageRecordId = record.Id,
                            AgentName = "AppointmentAgent",
                            AgentStatus = "Running",
                            StartedAt = DateTime.UtcNow,
                            InputPayload = JsonSerializer.Serialize(new { DoctorId = doctor.Id, Date = slot.Date, StartTime = slot.StartTime })
                        };
                        context.AgentWorkflows.Add(agentState);
                        
                        var agentResponse = await appointmentAgent.FindAndBookAsync(
                            doctor.Id, record.PatientId, slot.Date, slot.StartTime, slot.EndTime);
                            
                        try 
                        {
                            var apptDto = JsonSerializer.Deserialize<AppointmentDto>(agentResponse, 
                                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                            if (apptDto != null && apptDto.Id != Guid.Empty)
                            {
                                record.TentativeAppointmentId = apptDto.Id;
                                agentState.AgentStatus = "Completed";
                                agentState.OutputPayload = agentResponse;
                            }
                            else
                            {
                                agentState.AgentStatus = "Failed";
                                agentState.ErrorMessage = agentResponse;
                            }
                        }
                        catch 
                        { 
                            agentState.AgentStatus = "Failed";
                            agentState.ErrorMessage = agentResponse;
                        }
                        agentState.CompletedAt = DateTime.UtcNow;
                    }
                }
            }
        }
    }
}
