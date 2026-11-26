namespace PruebaCapitole.Service.Bookings.Queries
{
    public sealed record BookingDetailsDto(
        Guid Id,
        string ResourceId,
        string CustomerName,
        string CustomerEmail,
        DateTimeOffset Start,
        DateTimeOffset End,
        string Status,
        DateTimeOffset CreatedAt,
        DateTimeOffset? ConfirmedAt,
        DateTimeOffset? CancelledAt,
        string? CancellationReason);
}
