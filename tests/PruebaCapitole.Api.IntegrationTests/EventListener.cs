using System.Text.Json;
using RabbitMQ.Client;
using PruebaCapitole.Shared.IntegrationEvents;

namespace PruebaCapitole.Api.IntegrationTests;

/// <summary>
/// Plays the role of the consumer: binds a temporary queue to the events exchange
/// and reads the messages that the API publishes.
/// </summary>
public sealed class EventListener : IAsyncDisposable
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly string _queue;

    private EventListener(IConnection connection, IChannel channel, string queue)
    {
        _connection = connection;
        _channel = channel;
        _queue = queue;
    }

    /// <summary>Create it BEFORE the action under test: a queue only receives the messages published after it is bound.</summary>
    public static async Task<EventListener> StartAsync(ApiFixture api, string routingKey)
    {
        var factory = new ConnectionFactory
        {
            HostName = api.RabbitHost,
            Port = api.RabbitPort,
            UserName = api.RabbitUserName,
            Password = api.RabbitPasswordValue
        };

        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        // Same declaration as the publisher: if it differed, RabbitMQ would close the channel.
        await channel.ExchangeDeclareAsync(BookingEventsRouting.Exchange, ExchangeType.Topic, durable: true);
        var queue = (await channel.QueueDeclareAsync()).QueueName; // temporary, server-named queue
        await channel.QueueBindAsync(queue, BookingEventsRouting.Exchange, routingKey);

        return new EventListener(connection, channel, queue);
    }

    /// <summary>Waits until a message that satisfies <paramref name="match"/> arrives, or returns null on timeout.</summary>
    public async Task<T?> WaitForAsync<T>(Func<T, bool> match, TimeSpan timeout) where T : class
    {
        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            var message = await _channel.BasicGetAsync(_queue, autoAck: true);
            if (message is null)
            {
                await Task.Delay(200);
                continue;
            }

            var evt = JsonSerializer.Deserialize<T>(message.Body.Span, Json);
            if (evt is not null && match(evt)) return evt;
        }

        return null;
    }

    public async ValueTask DisposeAsync()
    {
        await _channel.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
