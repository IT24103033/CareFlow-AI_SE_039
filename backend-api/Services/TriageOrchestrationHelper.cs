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
            
            var availableSlots = new List<AvailableSlotDto>();
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
                            availableSlots.Add(slot);
                        }
                    }
                }
                if (availableSlots.Any()) 
                {
                    selectedDoctorId = doctor.Id;
                    break;
                }
            }

            if (!availableSlots.Any() || selectedDoctorId == null)
            {
                context.AgentWorkflows.Add(new AgentWorkflowState
                {
                    TriageRecordId = record.Id,
                    AgentName = "AppointmentAgent",
                    AgentStatus = "Failed",
                    StartedAt = DateTime.UtcNow,
                    CompletedAt = DateTime.UtcNow,
                    ErrorMessage = "No suitable slots found for any matching doctor."
                });
                return;
            }

            record.AssignedDoctorId = selectedDoctorId;

            var agentState = new AgentWorkflowState
            {
                TriageRecordId = record.Id,
                AgentName = "AppointmentAgent",
                AgentStatus = "ActionRequired",
                StartedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow,
                OutputPayload = JsonSerializer.Serialize(availableSlots)
            };
            context.AgentWorkflows.Add(agentState);
        }
    }
}
