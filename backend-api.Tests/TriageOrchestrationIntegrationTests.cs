using System;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CareFlowAI.API.Data;
using CareFlowAI.API.Models;
using CareFlowAI.API.Services;
using CareFlowAI.Orchestrator.Agents;
using CareFlowAI.Orchestrator.Tools;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Testcontainers.PostgreSql;
using Xunit;

namespace CareFlowAI.API.Tests;

public class TriageOrchestrationIntegrationTests : IDisposable
{
    private ApplicationDbContext _dbContext = null!;
    private IServiceProvider _serviceProvider = null!;

    public TriageOrchestrationIntegrationTests()
    {
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase(dbName));
        services.AddLogging();
        services.AddScoped<AppointmentService>();
        services.AddScoped<PharmacyAiService>();
        services.AddScoped<ISafetyAgent, PharmacyAiService>();

        services.AddHttpClient();
        services.AddScoped<FindAvailableSlotsTool>();
        services.AddScoped<CheckBookingConflictTool>();
        services.AddScoped<CreateTentativeBookingTool>();
        services.AddScoped<AppointmentActionAgent>();
        services.AddScoped<DoctorAvailabilityService>();
        services.AddScoped<CareFlowAI.Orchestrator.Abstractions.IAvailabilityProvider, AvailabilityProviderAdapter>();
        services.AddScoped<CareFlowAI.Orchestrator.Abstractions.IAppointmentProvider, AppointmentProviderAdapter>();
        services.AddScoped<CareFlowAI.Orchestrator.Abstractions.IAppointmentBookingProvider, AppointmentBookingProviderAdapter>();

        _serviceProvider = services.BuildServiceProvider();
        _dbContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();
        _dbContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task ExecuteDownstreamActionsAsync_ShouldPreventEmergencyAppointments_AndSetStatusFailed()
    {
        // Arrange
        var patient = new PatientProfile { FullName = "Emergency Patient" };
        var record = new TriageRecord
        {
            Patient = patient,
            Symptoms = "I am having a severe heart attack and chest pain.",
            SeverityLevel = "Critical",
            TriageStatus = "Pending"
        };
        _dbContext.TriageRecords.Add(record);
        await _dbContext.SaveChangesAsync();

        var plan = new CareFlowAI.API.Services.ClinicalPlan { UrgencyLevel = "Critical" };

        var appointmentAgent = _serviceProvider.GetRequiredService<AppointmentActionAgent>();
        var safetyAgent = _serviceProvider.GetRequiredService<ISafetyAgent>();

        // Act
        await TriageOrchestrationHelper.ExecuteDownstreamActionsAsync(
            record,
            plan,
            _dbContext,
            safetyAgent,
            appointmentAgent
        );
        
        await _dbContext.SaveChangesAsync();

        // Assert
        var safetyState = await _dbContext.AgentWorkflows
            .FirstOrDefaultAsync(w => w.TriageRecordId == record.Id && w.AgentName == "SafetyAgent");
            
        Assert.NotNull(safetyState);
        Assert.Equal("Completed", safetyState.AgentStatus);
        
        var appointmentState = await _dbContext.AgentWorkflows
            .FirstOrDefaultAsync(w => w.TriageRecordId == record.Id && w.AgentName == "AppointmentAgent");
            
        Assert.NotNull(appointmentState);
        Assert.Equal("Failed", appointmentState.AgentStatus);
        Assert.Contains("Emergency protocol activated", appointmentState.ErrorMessage);
    }
}
