namespace PruebaCapitole.Service.Bookings.Queries
{
    /// <summary>Read model del listado: las queries nunca devuelven entidades de dominio.</summary>
    public sealed record BookingListItemDto(
        Guid Id,
        string ResourceId,
        string CustomerName,
        string CustomerEmail,
        DateTimeOffset Start,
        DateTimeOffset End,
        string Status,
        DateTimeOffset CreatedAt);
}
