using System.Security.Claims;
using System.Text.Json;
using CareFlowAI.API.Controllers;
using CareFlowAI.API.Data;
using CareFlowAI.Orchestrator.Agents;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;
using CareFlowAI.API.Models;

namespace CareFlowAI.API.Tests;

public class AdmissionsControllerTests
{
    private class StubDomainAnalysisAgent : IDomainAnalysisAgent
    {
        private readonly Exception _exceptionToThrow;

        public StubDomainAnalysisAgent(Exception exceptionToThrow)
        {
            _exceptionToThrow = exceptionToThrow;
        }

        public Task<AgentOutput> AnalyzeRiskAsync(AgentInput input, CancellationToken cancellationToken)
        {
            throw _exceptionToThrow;
        }
    }

    private static ApplicationDbContext Context(string? name = null) => new(new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(name ?? Guid.NewGuid().ToString())
        .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning)).Options);

    private static AdmissionsController Controller(ApplicationDbContext db, IDomainAnalysisAgent agent, Guid? doctorId = null, string? role = "Doctor", bool authenticated = true)
    {
        var claims = new List<Claim>();
        if (role != null) claims.Add(new Claim(ClaimTypes.Role, role));
        if (doctorId != null) claims.Add(new Claim("doctor_id", doctorId.ToString()!));
        
        var controller = new AdmissionsController(db, new ConfigurationBuilder().Build(), agent)
        {
            ControllerContext = new ControllerContext 
            { 
                HttpContext = new DefaultHttpContext
                { 
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, authenticated ? "Test" : null)) 
                } 
            }
        };
        return controller;
    }

    [Fact]
    public async Task AnalyzePatientRisk_Returns503_WhenProviderUnavailable()
    {
        // Arrange
        var db = Context();
        var patient = new PatientProfile { FullName = "Test Patient" };
        db.PatientProfiles.Add(patient);
        await db.SaveChangesAsync();

        var agent = new StubDomainAnalysisAgent(new DomainAnalysisException("DOMAIN_PROVIDER_UNAVAILABLE", "Sensitive network timeout information 123"));

        var controller = Controller(db, agent);
        var input = new AgentInput { PatientId = patient.Id, CurrentSymptoms = "Cough" };

        // Act
        var result = await controller.AnalyzePatientRisk(input, CancellationToken.None);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(503, objectResult.StatusCode);
        var responseJson = JsonSerializer.Serialize(objectResult.Value);
        Assert.Contains("AI provider is temporarily unavailable", responseJson);
        Assert.DoesNotContain("Sensitive network", responseJson);
    }

    [Fact]
    public async Task AnalyzePatientRisk_Returns422_WhenOutputInvalid()
    {
        // Arrange
        var db = Context();
        var patient = new PatientProfile { FullName = "Test Patient" };
        db.PatientProfiles.Add(patient);
        await db.SaveChangesAsync();

        var agent = new StubDomainAnalysisAgent(new DomainAnalysisException("DOMAIN_INVALID_OUTPUT", "Sensitive parsing error 456"));

        var controller = Controller(db, agent);
        var input = new AgentInput { PatientId = patient.Id, CurrentSymptoms = "Cough" };

        // Act
        var result = await controller.AnalyzePatientRisk(input, CancellationToken.None);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(422, objectResult.StatusCode);
        var responseJson = JsonSerializer.Serialize(objectResult.Value);
        Assert.Contains("AI provider returned invalid", responseJson);
        Assert.DoesNotContain("Sensitive parsing error", responseJson);
    }
}
