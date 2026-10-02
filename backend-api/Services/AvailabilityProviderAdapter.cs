using CareFlowAI.Orchestrator.Abstractions;

namespace CareFlowAI.API.Services
{
    public class AvailabilityProviderAdapter : IAvailabilityProvider
    {
        private readonly DoctorAvailabilityService _availabilityService;

        public AvailabilityProviderAdapter(
            DoctorAvailabilityService availabilityService)
        {
            _availabilityService = availabilityService;
        }

        public async Task<List<AvailableSlotResult>> GetAvailableSlotsAsync(
            Guid doctorId,
            DateOnly date,
            int slotDurationMinutes = 30)
        {
            var slots = await _availabilityService.GetAvailableSlotsAsync(
                doctorId,
                date,
                slotDurationMinutes);

            return slots.Select(slot => new AvailableSlotResult
            {
                DoctorId = slot.DoctorId,
                DoctorName = slot.DoctorName,
                Specialization = slot.Specialization,
                Date = slot.Date,
                StartTime = slot.StartTime,
                EndTime = slot.EndTime
            }).ToList();
        }
    }
}