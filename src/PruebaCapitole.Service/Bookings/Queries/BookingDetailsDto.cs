namespace PruebaCapitole.Service.Bookings.Queries
{
    /// <summary>Read model del detalle (F5).</summary>
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
