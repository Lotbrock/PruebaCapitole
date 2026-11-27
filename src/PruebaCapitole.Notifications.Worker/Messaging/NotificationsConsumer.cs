using System.Text.Json;
using Microsoft.Extensions.Options;
using PruebaCapitole.Notifications.Worker.Data;
using PruebaCapitole.Shared.IntegrationEvents;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PruebaCapitole.Notifications.Worker.Messaging
{
    /// <summary>
    /// Consumes BookingConfirmedV1 and BookingCancelledV1.
    /// Manual ack: a message is removed from the queue only after it was recorded.
    /// On failure it is retried (exponential backoff) and, if it keeps failing, rejected to the DLQ.
    /// </summary>
    public sealed class NotificationsConsumer(
        IOptions<RabbitMqOptions> rabbitOptions,
        IOptions<RetryOptions> retryOptions,
        IServiceScopeFactory scopeFactory,
        TimeProvider clock,
        ILogger<NotificationsConsumer> logger) : BackgroundService
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
        private static readonly TimeSpan ConnectRetryDelay = TimeSpan.FromSeconds(5);

        private readonly RabbitMqOptions _rabbit = rabbitOptions.Value;
        private readonly RetryOptions _retry = retryOptions.Value;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var connection = await ConnectAsync(stoppingToken);
            if (connection is null)
                return;

            await using (connection)
            await using (var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken))
            {
                await RabbitMqTopology.DeclareAsync(channel, stoppingToken);

                // At most 10 unacknowledged messages at a time.
                await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 10, global: false, cancellationToken: stoppingToken);

                await StartConsumerAsync<BookingConfirmedV1>(channel, RabbitMqTopology.BookingConfirmedQueue, ToNotification, stoppingToken);
                await StartConsumerAsync<BookingCancelledV1>(channel, RabbitMqTopology.BookingCancelledQueue, ToNotification, stoppingToken);

                logger.LogInformation("Consuming from {ConfirmedQueue} and {CancelledQueue}",
                    RabbitMqTopology.BookingConfirmedQueue, RabbitMqTopology.BookingCancelledQueue);

                try { await Task.Delay(Timeout.Infinite, stoppingToken); }
                catch (OperationCanceledException) { /* host is shutting down */ }
            }
        }

        private async Task<IConnection?> ConnectAsync(CancellationToken ct)
        {
            var factory = new ConnectionFactory
            {
                HostName = _rabbit.Host,
                Port = _rabbit.Port,
                UserName = _rabbit.User,
                Password = _rabbit.Password,
                VirtualHost = _rabbit.VirtualHost,
                AutomaticRecoveryEnabled = true
            };

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    return await factory.CreateConnectionAsync(ct);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogWarning(ex, "RabbitMQ not available at {Host}:{Port}. Retrying in {Delay}s",
                        _rabbit.Host, _rabbit.Port, ConnectRetryDelay.TotalSeconds);
                    try { await Task.Delay(ConnectRetryDelay, ct); }
                    catch (OperationCanceledException) { }
                }
            }

            return null;
        }

        private Task StartConsumerAsync<TEvent>(
            IChannel channel, string queue, Func<TEvent, Notification> map, CancellationToken ct)
            where TEvent : class
        {
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += (_, ea) => HandleAsync(channel, ea, map, ct);
            return channel.BasicConsumeAsync(queue, autoAck: false, consumer, ct);
        }

        private async Task HandleAsync<TEvent>(
            IChannel channel, BasicDeliverEventArgs ea, Func<TEvent, Notification> map, CancellationToken ct)
            where TEvent : class
        {
            // A message that cannot be parsed will never work: no retries, straight to the DLQ.
            Notification notification;
            try
            {
                var message = JsonSerializer.Deserialize<TEvent>(ea.Body.Span, JsonOptions)
                    ?? throw new JsonException("Empty message.");
                notification = map(message);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Invalid message received on {RoutingKey}. Sent to the dead-letter queue", ea.RoutingKey);
                await channel.BasicRejectAsync(ea.DeliveryTag, requeue: false, ct);
                return;
            }

            // The CorrelationId travels in every log line written while processing this message.
            using var _ = logger.BeginScope(new Dictionary<string, object?> { ["CorrelationId"] = notification.CorrelationId });

            for (var attempt = 1; ; attempt++)
            {
                try
                {
                    using var scope = scopeFactory.CreateScope();
                    var recorder = scope.ServiceProvider.GetRequiredService<NotificationRecorder>();
                    await recorder.RecordAsync(notification, ct);

                    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, ct);
                    return;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    if (attempt >= _retry.MaxAttempts)
                    {
                        logger.LogError(ex, "Attempt {Attempt}/{MaxAttempts} failed for EventId={EventId}. Sent to the dead-letter queue",
                            attempt, _retry.MaxAttempts, notification.EventId);
                        await channel.BasicRejectAsync(ea.DeliveryTag, requeue: false, ct);
                        return;
                    }

                    var delay = TimeSpan.FromSeconds(_retry.BaseDelaySeconds * Math.Pow(2, attempt - 1));
                    logger.LogWarning(ex, "Attempt {Attempt}/{MaxAttempts} failed for EventId={EventId}. Retrying in {Delay}s",
                        attempt, _retry.MaxAttempts, notification.EventId, delay.TotalSeconds);
                    await Task.Delay(delay, ct);
                }
            }
        }

        private Notification ToNotification(BookingConfirmedV1 e) => new()
        {
            EventId = e.EventId,
            Type = "BookingConfirmed",
            BookingId = e.BookingId,
            ResourceId = e.ResourceId,
            CustomerEmail = e.CustomerEmail,
            Message = $"Your booking for {e.ResourceId} ({e.Start:u} to {e.End:u}) has been confirmed.",
            CorrelationId = e.CorrelationId,
            OccurredAtUtc = e.OccurredAtUtc,
            CreatedAtUtc = clock.GetUtcNow()
        };

        private Notification ToNotification(BookingCancelledV1 e) => new()
        {
            EventId = e.EventId,
            Type = "BookingCancelled",
            BookingId = e.BookingId,
            ResourceId = e.ResourceId,
            CustomerEmail = e.CustomerEmail,
            Message = string.IsNullOrWhiteSpace(e.Reason)
                ? $"Your booking for {e.ResourceId} ({e.Start:u} to {e.End:u}) has been cancelled."
                : $"Your booking for {e.ResourceId} ({e.Start:u} to {e.End:u}) has been cancelled. Reason: {e.Reason}",
            CorrelationId = e.CorrelationId,
            OccurredAtUtc = e.OccurredAtUtc,
            CreatedAtUtc = clock.GetUtcNow()
        };
    }
}
