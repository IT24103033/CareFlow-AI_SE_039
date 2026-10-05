namespace CareFlowAI.Orchestrator.Abstractions
{
    public interface IAppointmentProvider
    {
        Task<bool> CheckConflictAsync(
            Guid doctorId,
            DateOnly appointmentDate,
            TimeOnly startTime,
            TimeOnly endTime);
    }
}