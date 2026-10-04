using System.Net;
using System.Text;
using CareFlowAI.Orchestrator.Agents;
using CareFlowAI.Orchestrator.Handoff;
using CareFlowAI.Orchestrator.Tools;
using Xunit;

namespace CareFlowAI.API.Tests;

public class AgentHandoffTests
{
    [Fact]
    public async Task Valid_output_includes_patient_history_and_reaches_component_B()
    {
        var history = new StubHistory("Type 2 Diabetes, Hypertension.");
        var inbox = new RecordingHandoff();
        var json = """{"riskLevel":"High","flaggedFactors":["chest pain","Type 2 Diabetes"],"recommendedWardType":"ICU"}""";
        var http = new HttpClient(new GeminiHandler(json));
        var agent = new DomainAnalysisAgent("AIza-test", history, "gemini-test", http, inbox, TimeSpan.FromSeconds(5));

        var result = await agent.AnalyzeRiskAsync(new AgentInput
        {
            PatientName = "Marcus Thorne",
            CurrentSymptoms = "chest pain"
        });

        Assert.True(result.OutputValid);
        Assert.True(result.HandedOffToComponentB);
        Assert.Contains("Type 2 Diabetes", result.PatientHistoryUsed);
        Assert.Equal("High", result.RiskLevel);
        Assert.NotNull(inbox.Latest);
        Assert.Equal("Marcus Thorne", inbox.Latest!.PatientName);
        Assert.Equal("chest pain", inbox.Latest.CurrentSymptoms);
        Assert.Contains("Type 2 Diabetes", inbox.Latest.PatientHistory);
        Assert.True(inbox.Latest.OutputValid);
        Assert.Equal("ICU", inbox.Latest.RecommendedWardType);
    }

    [Fact]
    public async Task Invalid_model_json_is_rejected_and_still_handed_off()
    {
        var inbox = new RecordingHandoff();
        var http = new HttpClient(new GeminiHandler("this is not json"));
        var agent = new DomainAnalysisAgent("AIza-test", new StubHistory("None recorded"), "gemini-test", http, inbox);

        var result = await agent.AnalyzeRiskAsync(new AgentInput
        {
            PatientName = "Test",
            CurrentSymptoms = "cough"
        });

        Assert.False(result.OutputValid);
        Assert.Contains("Invalid agent output.", result.FlaggedFactors);
        Assert.True(result.HandedOffToComponentB);
        Assert.False(inbox.Latest!.OutputValid);
    }

    [Fact]
    public async Task Agent_timeout_is_reported_without_raw_exceptions()
    {
        var inbox = new RecordingHandoff();
        var http = new HttpClient(new DelayedHandler(TimeSpan.FromSeconds(2)))
        {
            Timeout = TimeSpan.FromMilliseconds(150)
        };
        var agent = new DomainAnalysisAgent("AIza-test", new StubHistory("Asthma"), "gemini-test", http, inbox);

        var result = await agent.AnalyzeRiskAsync(new AgentInput
        {
            PatientName = "Emily Chen",
            CurrentSymptoms = "wheeze"
        });

        Assert.False(result.OutputValid);
        Assert.Contains("Agent timeout.", result.FlaggedFactors);
        Assert.DoesNotContain(result.FlaggedFactors, f => f.Contains("Exception", StringComparison.OrdinalIgnoreCase));
        Assert.Equal("Asthma", result.PatientHistoryUsed);
        Assert.True(inbox.Latest!.OutputValid == false);
    }

    private sealed class StubHistory : IPatientHistoryTool
    {
        private readonly string _value;
        public StubHistory(string value) => _value = value;
        public Task<string> ExecuteAsync(string patientName) => Task.FromResult(_value);
    }

    private sealed class RecordingHandoff : IComponentBHandoff
    {
        public ComponentBHandoffPayload? Latest { get; private set; }

        public Task ReceiveAsync(ComponentBHandoffPayload payload, CancellationToken cancellationToken = default)
        {
            Latest = payload;
            return Task.CompletedTask;
        }
    }

    private sealed class GeminiHandler : HttpMessageHandler
    {
        private readonly string _modelText;
        public GeminiHandler(string modelText) => _modelText = modelText;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var escaped = _modelText.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n");
            var body = $"{{\"candidates\":[{{\"content\":{{\"parts\":[{{\"text\":\"{escaped}\"}}]}}}}]}}";
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            });
        }
    }

    private sealed class DelayedHandler : HttpMessageHandler
    {
        private readonly TimeSpan _delay;
        public DelayedHandler(TimeSpan delay) => _delay = delay;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await Task.Delay(_delay, cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
