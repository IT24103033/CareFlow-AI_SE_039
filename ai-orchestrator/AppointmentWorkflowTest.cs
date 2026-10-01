using CareFlowAI.Orchestrator;

namespace CareFlowAI.Orchestrator
{
    public class AppointmentWorkflowTest
    {
        public static async Task RunTestAsync()
        {
            var doctorId =
                Guid.Parse("3e7ca7f4-d733-494b-8e4e-254eca351930");

            var patientId =
                Guid.Parse("e97889df-ba7c-48a9-9c65-8324c39a7f59");

            var appointmentDate =
                new DateOnly(2026, 9, 25);

            var startTime =
                new TimeOnly(10, 0);

            var endTime =
                new TimeOnly(10, 30);

            var runner =
                new AppointmentWorkflowRunner();

            var result =
                await runner.RunAsync(
                    doctorId,
                    patientId,
                    appointmentDate,
                    startTime,
                    endTime);

            Console.WriteLine("=== Appointment Action Agent Test ===");
            Console.WriteLine(result);
        }
    }
}