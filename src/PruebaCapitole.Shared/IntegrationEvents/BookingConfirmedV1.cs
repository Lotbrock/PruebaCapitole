namespace PruebaCapitole.Shared.IntegrationEvents
{
    /// <summary>
    /// Public, versioned contract published when a booking is confirmed.
    /// It is independent from the domain event: the domain never knows the broker.
    /// </summary>
    public sealed record BookingConfirmedV1(
        Guid EventId,
        DateTimeOffset OccurredAtUtc,
        string CorrelationId,
        Guid BookingId,
        string ResourceId,
        DateTimeOffset Start,
        DateTimeOffset End,
        string CustomerEmail);
}
