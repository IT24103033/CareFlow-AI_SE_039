namespace CareFlowAI.API.DTOs
{
    public class SearchDoctorAvailabilityDto
    {
        public string? Specialization { get; set; }

        public Guid? DoctorId { get; set; }

        public DateOnly? Date { get; set; }
    }
}