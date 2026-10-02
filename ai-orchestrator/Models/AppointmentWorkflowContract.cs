namespace CareFlowAI.Orchestrator.Models
{
    public record AppointmentWorkflowRequest(
        Guid WorkflowId,
        Guid PatientId,
        Guid DoctorId,
        DateOnly AppointmentDate,
        TimeOnly StartTime,
        TimeOnly EndTime);

    public record AppointmentWorkflowResult(
        Guid WorkflowId,
        Guid? AppointmentId,
        AppointmentActionStatus Status,
        string Message);
}