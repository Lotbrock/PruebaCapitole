namespace PruebaCapitole.Infrastructure.Outbox
{
    /// <summary>A message waiting to be published (or already published) to the broker.</summary>
    public class OutboxMessage
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = null!;
        public string RoutingKey { get; set; } = null!;
        public string Payload { get; set; } = null!;
        public string CorrelationId { get; set; } = null!;
        public DateTimeOffset OccurredAtUtc { get; set; }
        public DateTimeOffset? ProcessedAtUtc { get; set; }
        public int Attempts { get; set; }
        public string? LastError { get; set; }
    }
}
