namespace PruebaCapitole.Notifications.Worker.Data
{
    public sealed class Notification
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public required Guid EventId { get; init; }
        public required string Type { get; init; }
        public required Guid BookingId { get; init; }
        public required string ResourceId { get; init; }
        public required string CustomerEmail { get; init; }
        public required string Message { get; init; }
        public string? CorrelationId { get; init; }
        public required DateTimeOffset OccurredAtUtc { get; init; }
        public required DateTimeOffset CreatedAtUtc { get; init; }
    }
}
