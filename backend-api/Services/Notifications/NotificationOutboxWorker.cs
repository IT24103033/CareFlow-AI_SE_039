using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CareFlowAI.API.Data;
using CareFlowAI.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CareFlowAI.API.Services.Notifications
{
    /// <summary>
    /// Drains the notification outbox: sends events whose inline attempt never ran (e.g. the process restarted)
    /// and retries transient failures (rate limit, timeout, 5xx) on a bounded backoff schedule.
    /// Uses the same claim logic as inline dispatch, so overlapping runs cannot double-send.
    /// </summary>
    public class NotificationOutboxWorker : BackgroundService
    {
        private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(15);

        private readonly IServiceScopeFactory _scopes;
        private readonly IOptionsMonitor<NotificationOptions> _options;
        private readonly ILogger<NotificationOutboxWorker> _logger;

        public NotificationOutboxWorker(
            IServiceScopeFactory scopes,
            IOptionsMonitor<NotificationOptions> options,
            ILogger<NotificationOutboxWorker> logger)
        {
            _scopes = scopes;
            _options = options;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_options.CurrentValue.WorkerEnabled) return;

            try { await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); }
            catch (OperationCanceledException) { return; }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessDueAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError("Notification outbox pass failed: {ErrorType}", ex.GetType().Name);
                }

                try { await Task.Delay(PollInterval, stoppingToken); }
                catch (OperationCanceledException) { return; }
            }
        }

        public async Task ProcessDueAsync(CancellationToken ct)
        {
            var max = _options.CurrentValue.MaxAutoAttempts;
            var now = DateTime.UtcNow;
            // Queued rows younger than this are left to the inline dispatch that is still running in the request.
            var inlineGrace = now - TimeSpan.FromSeconds(20);

            List<Guid> due;
            using (var scope = _scopes.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                due = await db.NotificationEvents.AsNoTracking()
                    .Where(e =>
                        (e.Status == NotificationStates.Queued && e.CreatedAt <= inlineGrace)
                        || (e.Status == NotificationStates.Failed && e.NextAttemptAt != null
                            && e.NextAttemptAt <= now && e.AttemptCount < max)
                        || (e.Status == NotificationStates.Sending && e.LockedUntil != null && e.LockedUntil < now))
                    .OrderBy(e => e.CreatedAt)
                    .Select(e => e.Id)
                    .Take(10)
                    .ToListAsync(ct);
            }

            foreach (var id in due)
            {
                ct.ThrowIfCancellationRequested();
                using var scope = _scopes.CreateScope();
                var dispatcher = scope.ServiceProvider.GetRequiredService<INotificationDispatcher>();
                await dispatcher.DispatchAsync(id, manual: false, ct);
                await Task.Delay(200, ct); // stay well under the provider's requests-per-second limit
            }
        }
    }
}
