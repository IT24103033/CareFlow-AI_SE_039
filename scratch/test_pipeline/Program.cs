using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using CareFlowAI.Orchestrator.Agents;
using CareFlowAI.API.Services;

class Program
{
    static async Task Main(string[] args)
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("/Users/sujana/Documents/CareFlow-AI_SE_039/backend-api/appsettings.Development.json")
            .Build();

        var apiKey = config["Gemini:ApiKey"];
        var modelName = config["Gemini:Model"];
        Console.WriteLine($"Model: {modelName}");

        var domainAdapter = new DummyDomainAdapter();
        var domainAgent = new DomainAnalysisAgent(domainAdapter, apiKey, modelName);

        var agentInput = new AgentInput { PatientId = Guid.NewGuid(), CurrentSymptoms = "I have severe chest pain and numbness in my left arm." };
        
        try 
        {
            Console.WriteLine("Running Domain Analysis (A)...");
            var domainOutput = await domainAgent.AnalyzeRiskAsync(agentInput);
            Console.WriteLine($"Risk: {domainOutput.RiskLevel}, Ward: {domainOutput.RecommendedWardType}");

            var client = new GeminiAssessmentClient(config);
            var assessInput = new ClinicalAssessmentInput(agentInput.CurrentSymptoms, "1 hour", "None", "O+", new DateOnly(1980, 1, 1), domainOutput.RiskLevel, domainOutput.FlaggedFactors);
            
            Console.WriteLine("Running Clinical Assessment (B)...");
            var result = await client.AssessAsync(assessInput, CancellationToken.None);
            Console.WriteLine("Result: " + result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}

class DummyDomainAdapter : IDomainContextAdapter
{
    public Task<AgentPatientProfile> GetPatientProfileAsync(Guid patientId, CancellationToken cancellationToken)
    {
        return Task.FromResult(new AgentPatientProfile { MedicalHistorySummary = "No major medical history." });
    }
}
