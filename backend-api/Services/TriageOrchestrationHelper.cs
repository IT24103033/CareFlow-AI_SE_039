using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
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
            // 1. D Validation (Emergency Rules)
            var safetyWorkflowState = safetyAgent.CheckEmergencyRules(record, plan);
            context.AgentWorkflows.Add(safetyWorkflowState);

            bool isEmergency = false;
            try
            {
                var safetyDoc = JsonDocument.Parse(safetyWorkflowState.OutputPayload);
                isEmergency = safetyDoc.RootElement.GetProperty("IsEmergency").GetBoolean();
            }
            catch { }

            if (isEmergency)
            {
                // Emergency cases must not receive a routine appointment.
                context.AgentWorkflows.Add(new AgentWorkflowState
                {
                    AgentName = "AppointmentAgent",
                    AgentStatus = "Failed",
                    ErrorMessage = "Emergency protocol activated: Automatic scheduling disabled.",
                    TriageRecordId = record.Id,
                    StartedAt = DateTime.UtcNow,
                    CompletedAt = DateTime.UtcNow
                });
                
                context.AgentWorkflows.Add(new AgentWorkflowState
                {
                    AgentName = "NotificationAgent",
                    AgentStatus = "Unsupported",
                    ErrorMessage = "Notification routing pending.",
                    TriageRecordId = record.Id,
                    StartedAt = DateTime.UtcNow,
                    CompletedAt = DateTime.UtcNow
                });
                
                return;
            }

            if (string.IsNullOrWhiteSpace(plan.SuggestedSpecialist))
            {
                return;
            }

            // Idempotency: reconcile orphaned bookings
            if (!record.TentativeAppointmentId.HasValue)
            {
                var orphanedAppt = await context.Appointments
                    .Where(a => a.PatientId == record.PatientId && a.Status == "Tentative" && a.CreatedAt >= DateTime.UtcNow.AddMinutes(-15))
                    .OrderByDescending(a => a.CreatedAt)
                    .FirstOrDefaultAsync();

                if (orphanedAppt != null)
                {
                    record.TentativeAppointmentId = orphanedAppt.Id;
                    return;
                }
            }
            else
            {
                return; // Already booked
            }

            // 2. C Action (Tentative Booking)
            var doctors = await context.Doctors
                .Where(d => d.Specialization == plan.SuggestedSpecialist && d.IsActive)
                .ToListAsync();

            if (!doctors.Any())
            {
                context.AgentWorkflows.Add(new AgentWorkflowState
                {
                    TriageRecordId = record.Id,
                    AgentName = "AppointmentAgent",
                    AgentStatus = "Failed",
                    StartedAt = DateTime.UtcNow,
                    CompletedAt = DateTime.UtcNow,
                    ErrorMessage = "No matching doctor found for the suggested specialty."
                });
                return;
            }

            var availService = new DoctorAvailabilityService(context);
            var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
            var end = tomorrow.AddDays(7);
            
            AvailableSlotDto? selectedSlot = null;
            Guid? selectedDoctorId = null;

            foreach (var doctor in doctors)
            {
                for (var date = tomorrow; date <= end; date = date.AddDays(1))
                {
                    var slots = await availService.GetAvailableSlotsAsync(doctor.Id, date, 30);
                    foreach (var slot in slots)
                    {
                        var hasConflict = await context.Appointments.AnyAsync(a => 
                            a.DoctorId == doctor.Id && 
                            a.AppointmentDate == date && 
                            a.Status != "Cancelled" &&
                            slot.StartTime < a.EndTime && 
                            slot.EndTime > a.StartTime);

                        if (!hasConflict)
                        {
                            selectedSlot = slot;
                            selectedDoctorId = doctor.Id;
                            break;
                        }
                    }
                    if (selectedSlot != null) break;
                }
                if (selectedSlot != null) break;
            }

            if (selectedSlot == null || selectedDoctorId == null)
            {
                context.AgentWorkflows.Add(new AgentWorkflowState
                {
                    TriageRecordId = record.Id,
                    AgentName = "AppointmentAgent",
                    AgentStatus = "Failed",
                    StartedAt = DateTime.UtcNow,
                    CompletedAt = DateTime.UtcNow,
                    ErrorMessage = "No suitable slot found for any matching doctor."
                });
                return;
            }

            var agentState = new AgentWorkflowState
            {
                TriageRecordId = record.Id,
                AgentName = "AppointmentAgent",
                AgentStatus = "Running",
                StartedAt = DateTime.UtcNow,
                InputPayload = JsonSerializer.Serialize(new { 
                    DoctorId = selectedDoctorId.Value, 
                    Date = selectedSlot.Date, 
                    StartTime = selectedSlot.StartTime, 
                    EndTime = selectedSlot.EndTime 
                })
            };
            context.AgentWorkflows.Add(agentState);
            
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            
            try 
            {
                var task = appointmentAgent.FindAndBookAsync(
                    selectedDoctorId.Value, record.PatientId, selectedSlot.Date, selectedSlot.StartTime, selectedSlot.EndTime);
                
                var agentResponse = await task.WaitAsync(cts.Token);
                
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
            }
            catch (Exception ex) when (ex is OperationCanceledException || ex is TimeoutException)
            {
                agentState.AgentStatus = "Failed";
                agentState.ErrorMessage = "Execution timed out.";
            }
            catch (Exception ex)
            { 
                agentState.AgentStatus = "Failed";
                agentState.ErrorMessage = "Execution failure: " + ex.Message;
            }
            agentState.CompletedAt = DateTime.UtcNow;
        }
    }
}
