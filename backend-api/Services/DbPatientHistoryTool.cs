using CareFlowAI.API.Data;
using CareFlowAI.API.Models;
using CareFlowAI.Orchestrator.Tools;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace CareFlowAI.API.Services
{
    public class DbPatientHistoryTool : IPatientHistoryTool
    {
        private readonly ApplicationDbContext _context;

        public DbPatientHistoryTool(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> ExecuteAsync(string patientName)
        {
            if (string.IsNullOrWhiteSpace(patientName))
            {
                return "No patient name was provided, so stored medical history could not be loaded.";
            }

            List<PatientProfile> matches;
            try
            {
                matches = await _context.PatientProfiles
                    .AsNoTracking()
                    .Where(p => EF.Functions.ILike(p.FullName, $"%{patientName.Trim()}%"))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return $"Could not load stored medical history from the database: {ex.Message}";
            }

            if (matches.Count == 0)
            {
                return $"No CareFlow patient record matched '{patientName}'. Stored medical history is unavailable.";
            }

            var builder = new StringBuilder();
            foreach (var patient in matches)
            {
                var history = string.IsNullOrWhiteSpace(patient.MedicalHistorySummary)
                    ? "None recorded"
                    : patient.MedicalHistorySummary;

                builder.AppendLine($"Patient: {patient.FullName}");
                builder.AppendLine($"Date of birth: {patient.DateOfBirth:yyyy-MM-dd}");
                builder.AppendLine($"Blood group: {(string.IsNullOrWhiteSpace(patient.BloodGroup) ? "Unknown" : patient.BloodGroup)}");
                builder.AppendLine($"Medical history: {history}");
                builder.AppendLine();
            }

            return builder.ToString().Trim();
        }
    }
}
