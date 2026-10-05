namespace CareFlowAI.Orchestrator.Abstractions
{
    public interface IAppointmentBookingProvider
    {
        Task<string> CreateTentativeAsync(
            Guid doctorId,
            Guid patientId,
            DateOnly appointmentDate,
            TimeOnly startTime,
            TimeOnly endTime);
    }
}