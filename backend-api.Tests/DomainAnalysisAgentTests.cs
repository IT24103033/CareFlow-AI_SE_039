using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CareFlowAI.Orchestrator.Agents;
using Xunit;

namespace CareFlowAI.API.Tests
{
    public class DomainAnalysisAgentTests
    {
        private class StubContextAdapter : IDomainContextAdapter
        {
            public Task<CareFlowAI.Orchestrator.Agents.AgentPatientProfile?> GetPatientProfileAsync(Guid patientId, CancellationToken cancellationToken)
            {
                return Task.FromResult<CareFlowAI.Orchestrator.Agents.AgentPatientProfile?>(
                    new CareFlowAI.Orchestrator.Agents.AgentPatientProfile { MedicalHistorySummary = "History" }
                );
            }
        }

        private class FakeHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

            public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
            {
                _handler = handler;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                // This will throw if the token is already canceled, proving cancellation is observed by the client.
                cancellationToken.ThrowIfCancellationRequested();
                return Task.FromResult(_handler(request));
            }
        }

        [Fact]
        public async Task Agent_recovers_after_transient_provider_overload()
        {
            var calls = 0;
            var handler = new FakeHttpMessageHandler(_ =>
            {
                calls++;
                if (calls == 1) return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
                var assessment = System.Text.Json.JsonSerializer.Serialize(new {
                    riskLevel = "Medium", recommendedWardType = "General", flaggedFactors = new[] { "Synthetic test" }
                });
                var body = System.Text.Json.JsonSerializer.Serialize(new {
                    candidates = new[] { new { content = new { parts = new[] { new { text = assessment } } } } }
                });
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body) };
            });
            var agent = new DomainAnalysisAgent(new StubContextAdapter(), "test-key", "test-model", new HttpClient(handler));
            var result = await agent.AnalyzeRiskAsync(new AgentInput { PatientId = Guid.NewGuid(), CurrentSymptoms = "Synthetic dental discomfort" });
            Assert.Equal("Medium", result.RiskLevel);
            Assert.Equal(2, calls);
        }

        [Fact]
        public async Task Agent_throws_on_http_error_rather_than_silently_defaulting_to_HighRisk()
        {
            var rawErrorBody = "SENSITIVE_PROVIDER_TEXT_12345";
            var handler = new FakeHttpMessageHandler(req => new HttpResponseMessage(HttpStatusCode.InternalServerError) 
            { 
                Content = new StringContent(rawErrorBody) 
            });
            var client = new HttpClient(handler);
            var agent = new DomainAnalysisAgent(new StubContextAdapter(), "api-key", "gemini-3.8-flash", client);

            var input = new AgentInput { PatientId = Guid.NewGuid(), CurrentSymptoms = "Cough" };

            var ex = await Assert.ThrowsAsync<DomainAnalysisException>(async () => await agent.AnalyzeRiskAsync(input));
            Assert.Equal("DOMAIN_PROVIDER_UNAVAILABLE", ex.ErrorCode);
            Assert.DoesNotContain(rawErrorBody, ex.Message);
            Assert.DoesNotContain("api-key", ex.Message);
        }

        [Fact]
        public async Task Agent_throws_on_malformed_json_rather_than_silently_defaulting()
        {
            var handler = new FakeHttpMessageHandler(req => new HttpResponseMessage(HttpStatusCode.OK) 
            { 
                Content = new StringContent("invalid json") 
            });
            var client = new HttpClient(handler);
            var agent = new DomainAnalysisAgent(new StubContextAdapter(), "api-key", "gemini-3.8-flash", client);

            var input = new AgentInput { PatientId = Guid.NewGuid(), CurrentSymptoms = "Cough" };

            var ex = await Assert.ThrowsAsync<DomainAnalysisException>(async () => await agent.AnalyzeRiskAsync(input));
            Assert.Equal("DOMAIN_INVALID_OUTPUT", ex.ErrorCode);
        }

        [Fact]
        public async Task Agent_throws_on_invalid_risk_level_rather_than_defaulting()
        {
            var handler = new FakeHttpMessageHandler(req => new HttpResponseMessage(HttpStatusCode.OK) 
            { 
                Content = new StringContent("{ \"candidates\": [ { \"content\": { \"parts\": [ { \"text\": \"{\\\"riskLevel\\\": \\\"Extreme\\\", \\\"recommendedWardType\\\": \\\"General\\\", \\\"flaggedFactors\\\": []}\" } ] } } ] }") 
            });
            var client = new HttpClient(handler);
            var agent = new DomainAnalysisAgent(new StubContextAdapter(), "api-key", "gemini-3.8-flash", client);

            var input = new AgentInput { PatientId = Guid.NewGuid(), CurrentSymptoms = "Cough" };

            var ex = await Assert.ThrowsAsync<DomainAnalysisException>(async () => await agent.AnalyzeRiskAsync(input));
            Assert.Equal("DOMAIN_INVALID_OUTPUT", ex.ErrorCode);
        }

        [Fact]
        public async Task Agent_observes_cancellation_token()
        {
            var handler = new FakeHttpMessageHandler(req => new HttpResponseMessage(HttpStatusCode.OK));
            var client = new HttpClient(handler);
            var agent = new DomainAnalysisAgent(new StubContextAdapter(), "api-key", "gemini-3.8-flash", client);

            var input = new AgentInput { PatientId = Guid.NewGuid(), CurrentSymptoms = "Cough" };
            
            var cts = new CancellationTokenSource();
            cts.Cancel(); // Pre-cancel

            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await agent.AnalyzeRiskAsync(input, cts.Token));
        }
    }
}
