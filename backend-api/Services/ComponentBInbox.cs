using System.Collections.Concurrent;
using CareFlowAI.Orchestrator.Handoff;

namespace CareFlowAI.API.Services
{
    public class ComponentBInbox : IComponentBHandoff
    {
        public ConcurrentQueue<ComponentBHandoffPayload> Messages { get; } = new();

        public Task ReceiveAsync(ComponentBHandoffPayload payload, CancellationToken cancellationToken = default)
        {
            Messages.Enqueue(payload);
            return Task.CompletedTask;
        }
    }
}
