using CareFlowAI.API.DTOs;
using CareFlowAI.Orchestrator.Abstractions;

namespace CareFlowAI.API.Services
{
    public class AppointmentBookingProviderAdapter
        : IAppointmentBookingProvider
    {
        private readonly AppointmentService _appointmentService;

        public AppointmentBookingProviderAdapter(
            AppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        public async Task<string> CreateTentativeAsync(
            Guid doctorId,
            Guid patientId,
            DateOnly appointmentDate,
            TimeOnly startTime,
            TimeOnly endTime)
        {
            var dto = new CreateAppointmentDto
            {
                DoctorId = doctorId,
                PatientId = patientId,
                AppointmentDate = appointmentDate,
                StartTime = startTime,
                EndTime = endTime
            };

            var appointment =
                await _appointmentService.CreateTentativeAsync(dto);

            if (appointment == null)
            {
                return "Unable to create the tentative appointment.";
            }

            return System.Text.Json.JsonSerializer.Serialize(appointment);
        }
    }
}