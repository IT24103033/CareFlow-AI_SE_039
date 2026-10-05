namespace CareFlowAI.Orchestrator.Models
{
    public enum AppointmentActionStatus
    {
        Success,
        Unavailable,
        Conflict,
        InvalidRequest,
        ProviderError
    }

    public class AppointmentActionResult
    {
        public AppointmentActionStatus Status { get; set; }

        public string Message { get; set; } = string.Empty;

        public Guid? AppointmentId { get; set; }

        public static AppointmentActionResult Success(
            string message,
            Guid? appointmentId = null)
        {
            return new AppointmentActionResult
            {
                Status = AppointmentActionStatus.Success,
                Message = message,
                AppointmentId = appointmentId
            };
        }

        public static AppointmentActionResult Unavailable(
            string message)
        {
            return new AppointmentActionResult
            {
                Status = AppointmentActionStatus.Unavailable,
                Message = message
            };
        }

        public static AppointmentActionResult Conflict(
            string message)
        {
            return new AppointmentActionResult
            {
                Status = AppointmentActionStatus.Conflict,
                Message = message
            };
        }

        public static AppointmentActionResult InvalidRequest(
            string message)
        {
            return new AppointmentActionResult
            {
                Status = AppointmentActionStatus.InvalidRequest,
                Message = message
            };
        }

        public static AppointmentActionResult ProviderError(
            string message)
        {
            return new AppointmentActionResult
            {
                Status = AppointmentActionStatus.ProviderError,
                Message = message
            };
        }
    }
}