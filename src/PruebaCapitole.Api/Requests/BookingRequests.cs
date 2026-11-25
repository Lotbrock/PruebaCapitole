namespace PruebaCapitole.Api.Requests
{
    public sealed record CreateBookingRequest(
        string ResourceId,
        string CustomerName,
        string CustomerEmail,
        DateTimeOffset Start,
        DateTimeOffset End);

    public sealed record CancelBookingRequest(string? Reason);
}
