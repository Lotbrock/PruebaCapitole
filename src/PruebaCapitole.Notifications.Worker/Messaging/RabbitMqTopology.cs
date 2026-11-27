using RabbitMQ.Client;

namespace PruebaCapitole.Notifications.Worker.Messaging
{
    /// <summary>
    /// Exchanges, queues and bindings used by the worker.
    /// Declaring them is idempotent: it is safe to run on every start.
    /// </summary>
    public static class RabbitMqTopology
    {
        // Main exchange: the API (Outbox publisher) publishes here.
        public const string EventsExchange = "bookings.events";
        // Dead-letter exchange: messages that fail persistently end up here.
        public const string DeadLetterExchange = "bookings.events.dlx";

        public const string BookingConfirmedKey = "booking.confirmed.v1";
        public const string BookingCancelledKey = "booking.cancelled.v1";

        public const string BookingConfirmedQueue = "notifications.booking-confirmed.v1";
        public const string BookingCancelledQueue = "notifications.booking-cancelled.v1";

        public static string DeadLetterQueue(string queue) => $"{queue}.dlq";

        public static async Task DeclareAsync(IChannel channel, CancellationToken ct)
        {
            await channel.ExchangeDeclareAsync(EventsExchange, ExchangeType.Topic, durable: true, cancellationToken: ct);
            await channel.ExchangeDeclareAsync(DeadLetterExchange, ExchangeType.Direct, durable: true, cancellationToken: ct);

            await DeclareQueueAsync(channel, BookingConfirmedQueue, BookingConfirmedKey, ct);
            await DeclareQueueAsync(channel, BookingCancelledQueue, BookingCancelledKey, ct);
        }

        private static async Task DeclareQueueAsync(IChannel channel, string queue, string routingKey, CancellationToken ct)
        {
            var dlq = DeadLetterQueue(queue);

            // Dead-letter queue: where rejected messages are parked for inspection.
            await channel.QueueDeclareAsync(dlq, durable: true, exclusive: false, autoDelete: false, cancellationToken: ct);
            await channel.QueueBindAsync(dlq, DeadLetterExchange, routingKey: dlq, cancellationToken: ct);

            // Work queue: rejected messages go to the DLX using the DLQ name as routing key.
            var arguments = new Dictionary<string, object?>
            {
                ["x-dead-letter-exchange"] = DeadLetterExchange,
                ["x-dead-letter-routing-key"] = dlq
            };
            await channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false, arguments: arguments, cancellationToken: ct);
            await channel.QueueBindAsync(queue, EventsExchange, routingKey, cancellationToken: ct);
        }
    }
}
