namespace CareFlowAI.API.Models
{
    public class ApiErrorResponse
    {
        public string Error { get; set; } = string.Empty;
        public Guid CorrelationId { get; set; }
    }
}
