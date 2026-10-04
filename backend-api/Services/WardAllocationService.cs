using System.Data;
using CareFlowAI.API.Data;
using CareFlowAI.API.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CareFlowAI.API.Services
{
    public class AllocationOutcome
    {
        public bool Success { get; init; }
        public int StatusCode { get; init; }
        public string? Error { get; init; }
        public Admission? Admission { get; init; }

        public static AllocationOutcome Ok(Admission admission) =>
            new() { Success = true, StatusCode = 200, Admission = admission };

        public static AllocationOutcome Fail(int status, string error) =>
            new() { Success = false, StatusCode = status, Error = error };
    }

    public class WardAllocationService
    {
        private readonly ApplicationDbContext _context;

        public WardAllocationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AllocationOutcome> AllocateAsync(AdmissionRequestDto request, CancellationToken cancellationToken = default)
        {
            var patient = await _context.PatientProfiles.FindAsync(new object[] { request.PatientProfileId }, cancellationToken);
            if (patient == null)
                return AllocationOutcome.Fail(404, "Patient not found.");

            var wardExists = await _context.Wards.AnyAsync(w => w.Id == request.WardId, cancellationToken);
            if (!wardExists)
                return AllocationOutcome.Fail(404, "Ward not found.");

            await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            try
            {
                var hasActive = await _context.Admissions.AnyAsync(
                    a => a.PatientProfileId == request.PatientProfileId && a.DischargedAt == null,
                    cancellationToken);
                if (hasActive)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return AllocationOutcome.Fail(409, "Patient already has an active admission.");
                }

                var reserved = await _context.Wards
                    .Where(w => w.Id == request.WardId && w.OccupiedBeds < w.Capacity)
                    .ExecuteUpdateAsync(
                        setters => setters.SetProperty(w => w.OccupiedBeds, w => w.OccupiedBeds + 1),
                        cancellationToken);

                if (reserved == 0)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return AllocationOutcome.Fail(400, "Ward is full. Cannot allocate bed.");
                }

                var admission = new Admission
                {
                    PatientProfileId = request.PatientProfileId,
                    WardId = request.WardId,
                    RiskLevel = string.IsNullOrWhiteSpace(request.RiskLevel)
                        ? "Pending AI Analysis"
                        : request.RiskLevel.Trim()
                };

                _context.Admissions.Add(admission);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return AllocationOutcome.Ok(admission);
            }
            catch (PostgresException ex) when (ex.SqlState is "40001" or "40P01")
            {
                await transaction.RollbackAsync(cancellationToken);
                return AllocationOutcome.Fail(409, "Could not complete allocation due to a concurrent update. Retry the request.");
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
