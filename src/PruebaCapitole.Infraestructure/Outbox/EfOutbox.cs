using System.Text.Json;
using PruebaCapitole.Infrastructure.Data;
using PruebaCapitole.Service.Abstractions;

namespace PruebaCapitole.Infrastructure.Outbox
{
    /// <summary>Adapter of IOutbox: only tracks the row; the caller's SaveChanges commits it.</summary>
    public sealed class EfOutbox(CapitoleDbContext db, TimeProvider clock) : IOutbox
    {
        public void Add(string routingKey, object integrationEvent, string correlationId)
        {
            db.OutboxMessages.Add(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = integrationEvent.GetType().Name,
                RoutingKey = routingKey,
                Payload = JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType(), OutboxJson.Options),
                CorrelationId = correlationId,
                OccurredAtUtc = clock.GetUtcNow()
            });
        }
    }

    internal static class OutboxJson
    {
        public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
    }
}
