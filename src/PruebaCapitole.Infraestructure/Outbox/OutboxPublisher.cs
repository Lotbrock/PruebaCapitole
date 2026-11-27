using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PruebaCapitole.Infrastructure.Data;
using PruebaCapitole.Infrastructure.Messaging;
using PruebaCapitole.Shared.IntegrationEvents;
using RabbitMQ.Client;

namespace PruebaCapitole.Infrastructure.Outbox
{
    /// <summary>
    /// Background process: every few seconds it reads pending OutboxMessages, publishes them
    /// to RabbitMQ and marks them as processed. If the broker is down the rows simply stay
    /// pending and are retried on the next cycle (at-least-once delivery).
    /// </summary>
    public sealed class OutboxPublisher(
        IServiceScopeFactory scopeFactory,
        IOptions<OutboxOptions> outboxOptions,
        IOptions<RabbitMqOptions> rabbitOptions,
        TimeProvider clock,
        ILogger<OutboxPublisher> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(outboxOptions.Value.PollingIntervalSeconds));
            do
            {
                try
                {
                    await PublishPendingAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Outbox cycle failed; it will be retried.");
                }
            }
            while (await WaitAsync(timer, stoppingToken));
        }

        private static async Task<bool> WaitAsync(PeriodicTimer timer, CancellationToken ct)
        {
            try { return await timer.WaitForNextTickAsync(ct); }
            catch (OperationCanceledException) { return false; }
        }

        private async Task PublishPendingAsync(CancellationToken ct)
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CapitoleDbContext>();

            var pending = await db.OutboxMessages
                .Where(m => m.ProcessedAtUtc == null)
                .OrderBy(m => m.OccurredAtUtc)
                .Take(outboxOptions.Value.BatchSize)
                .ToListAsync(ct);

            if (pending.Count == 0) return;

            var rabbit = rabbitOptions.Value;
            var factory = new ConnectionFactory
            {
                HostName = rabbit.Host,
                Port = rabbit.Port,
                UserName = rabbit.User,
                Password = rabbit.Password,
                VirtualHost = rabbit.VirtualHost
            };

            await using var connection = await factory.CreateConnectionAsync(ct);
            // Confirms + tracking: BasicPublishAsync throws if the broker nacks or the message is unroutable.
            await using var channel = await connection.CreateChannelAsync(
                new CreateChannelOptions(publisherConfirmationsEnabled: true, publisherConfirmationTrackingEnabled: true),
                ct);
            await channel.ExchangeDeclareAsync(BookingEventsRouting.Exchange, ExchangeType.Topic, durable: true, cancellationToken: ct);

            try
            {
                foreach (var message in pending)
                {
                    try
                    {
                        var props = new BasicProperties
                        {
                            Persistent = true,
                            ContentType = "application/json",
                            MessageId = message.Id.ToString(),
                            CorrelationId = message.CorrelationId,
                            Type = message.Type
                        };

                        await channel.BasicPublishAsync(
                            BookingEventsRouting.Exchange, message.RoutingKey, mandatory: true,
                            basicProperties: props, body: Encoding.UTF8.GetBytes(message.Payload), cancellationToken: ct);

                        message.ProcessedAtUtc = clock.GetUtcNow();
                        message.LastError = null;
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        message.Attempts++;
                        message.LastError = ex.Message.Length > 2000 ? ex.Message[..2000] : ex.Message;
                        logger.LogWarning(ex, "Could not publish outbox message {Id} (CorrelationId {CorrelationId}).",
                            message.Id, message.CorrelationId);
                        break; // keep order; the rest waits for the next cycle
                    }
                }
            }
            finally
            {
                // Save what was published even if the loop was interrupted.
                await db.SaveChangesAsync(CancellationToken.None);
            }
        }
    }
}
