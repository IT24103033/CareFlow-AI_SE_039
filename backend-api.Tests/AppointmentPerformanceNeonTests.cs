
using System.Diagnostics;
using CareFlowAI.API.Data;
using CareFlowAI.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit.Abstractions;

namespace CareFlowAI.API.Tests;

public class AppointmentPerformanceNeonTests
{
    private readonly ITestOutputHelper _output;

    public AppointmentPerformanceNeonTests(
        ITestOutputHelper output)
    {
        _output = output;
    }

    private static string GetConnectionString()
    {
        var environmentConnection =
            Environment.GetEnvironmentVariable(
                "ConnectionStrings__DefaultConnection");

        if (!string.IsNullOrWhiteSpace(environmentConnection))
        {
            return environmentConnection;
        }

        var directory =
            new DirectoryInfo(AppContext.BaseDirectory);

        while (directory != null &&
               !File.Exists(
                   Path.Combine(
                       directory.FullName,
                       "CareFlow-AI_SE_039.sln")))
        {
            directory = directory.Parent;
        }

        if (directory == null)
        {
            throw new InvalidOperationException(
                "Could not locate the CareFlow-AI_SE_039 solution root.");
        }

        var configuration =
            new ConfigurationBuilder()
                .SetBasePath(directory.FullName)
                .AddJsonFile(
                    "backend-api/appsettings.json",
                    optional: false,
                    reloadOnChange: false)
                .AddJsonFile(
                    "backend-api/appsettings.Development.json",
                    optional: true,
                    reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

        var connectionString =
            configuration.GetConnectionString(
                "DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "No DefaultConnection was found for Neon performance testing.");
        }

        if (connectionString.Contains(
                "YOUR_",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "The Neon connection string still contains a placeholder.");
        }

        return connectionString;
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options =
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(GetConnectionString())
                .Options;

        return new ApplicationDbContext(options);
    }

    [Fact(Skip = "Manual Neon performance test; enable only with a configured test database.")]
    public async Task Neon_DoctorAvailability_Performance_10SequentialReads()
    {
        await using var setupDb =
            CreateDbContext();

        var availability =
            await setupDb.DoctorAvailabilities
                .AsNoTracking()
                .OrderBy(a => a.Date)
                .ThenBy(a => a.StartTime)
                .FirstOrDefaultAsync();

        Assert.NotNull(availability);

        await using var serviceDb = CreateDbContext();

        var service =
            new DoctorAvailabilityService(serviceDb);

        // Warm up before collecting performance measurements.
        var warmup =
            await service.GetAvailableSlotsAsync(
                availability!.DoctorId,
                availability.Date,
                30);

        Assert.NotNull(warmup);

        var timings = new List<long>();

        for (var i = 0; i < 10; i++)
        {
            var stopwatch = Stopwatch.StartNew();

            var slots =
                await service.GetAvailableSlotsAsync(
                    availability.DoctorId,
                    availability.Date,
                    30);

            stopwatch.Stop();

            Assert.NotNull(slots);
            timings.Add(stopwatch.ElapsedMilliseconds);
        }

        _output.WriteLine(
            "Neon PostgreSQL availability performance - 10 sequential reads");

        _output.WriteLine($"Minimum: {timings.Min()} ms");
        _output.WriteLine($"Average: {timings.Average():F2} ms");
        _output.WriteLine($"Maximum: {timings.Max()} ms");
        _output.WriteLine($"Samples: {timings.Count}");

        Assert.Equal(10, timings.Count);

        Assert.All(
            timings,
            timing => Assert.True(timing >= 0));
    }

    [Fact(Skip = "Manual Neon performance test; enable only with a configured test database.")]
    public async Task Neon_DoctorAvailability_Performance_10ConcurrentReads()
    {
        await using var setupDb = CreateDbContext();

        var availability =
            await setupDb.DoctorAvailabilities
                .AsNoTracking()
                .OrderBy(a => a.Date)
                .ThenBy(a => a.StartTime)
                .FirstOrDefaultAsync();

        Assert.NotNull(availability);

        var stopwatch = Stopwatch.StartNew();

        var tasks =
            Enumerable.Range(0, 10)
                .Select(async _ =>
                {
                    await using var db = CreateDbContext();

                    var service =
                        new DoctorAvailabilityService(db);

                    var callStopwatch = Stopwatch.StartNew();

                    var slots =
                        await service.GetAvailableSlotsAsync(
                            availability!.DoctorId,
                            availability.Date,
                            30);

                    callStopwatch.Stop();

                    return new
                    {
                        Slots = slots,
                        ElapsedMilliseconds =
                            callStopwatch.ElapsedMilliseconds
                    };
                })
                .ToArray();

        var results = await Task.WhenAll(tasks);

        stopwatch.Stop();

        Assert.Equal(10, results.Length);

        Assert.All(
            results,
            result => Assert.NotNull(result.Slots));

        _output.WriteLine(
            "Neon PostgreSQL availability performance - 10 concurrent reads");

        _output.WriteLine(
            $"Total concurrent execution time: {stopwatch.ElapsedMilliseconds} ms");

        _output.WriteLine(
            $"Average individual request time: {results.Average(r => r.ElapsedMilliseconds):F2} ms");

        _output.WriteLine(
            $"Maximum individual request time: {results.Max(r => r.ElapsedMilliseconds)} ms");

        _output.WriteLine($"Concurrent samples: {results.Length}");

        Assert.True(stopwatch.ElapsedMilliseconds >= 0);
    }
}
