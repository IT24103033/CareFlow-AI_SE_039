namespace CareFlowAI.Orchestrator.Abstractions
{
    public interface IAvailabilityProvider
    {
        Task<List<AvailableSlotResult>> GetAvailableSlotsAsync(
            Guid doctorId,
            DateOnly date,
            int slotDurationMinutes = 30);
    }

    public class AvailableSlotResult
    {
        public Guid DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }
}