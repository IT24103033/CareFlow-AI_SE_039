using CareFlowAI.API.Data;
using Microsoft.EntityFrameworkCore;

namespace CareFlowAI.API.Services;

public class PatientContextTool(ApplicationDbContext context) : IPatientContextTool
{
    public async Task<PatientContextSnapshot?> GetPatientProfileAsync(Guid patientId, CancellationToken cancellationToken)
    {
        if (patientId == Guid.Empty) throw new ArgumentException("A patient ID is required.", nameof(patientId));
        // Read-only projection. No arbitrary SQL, names, contact details or other patients.
        return await context.PatientProfiles.AsNoTracking().Where(p => p.Id == patientId)
            .Select(p => new PatientContextSnapshot(p.Id, p.MedicalHistorySummary))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
