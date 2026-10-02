using CareFlowAI.API.DTOs;
using CareFlowAI.Orchestrator.Abstractions;

namespace CareFlowAI.API.Services
{
    public class AppointmentProviderAdapter : IAppointmentProvider
    {
        private readonly AppointmentService _appointmentService;

        public AppointmentProviderAdapter(
            AppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        public async Task<bool> CheckConflictAsync(
            Guid doctorId,
            DateOnly appointmentDate,
            TimeOnly startTime,
            TimeOnly endTime)
        {
            return await _appointmentService.CheckConflictAsync(
                doctorId,
                appointmentDate,
                startTime,
                endTime);
        }
    }
}