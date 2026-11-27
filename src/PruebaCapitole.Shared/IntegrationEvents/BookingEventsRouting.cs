namespace PruebaCapitole.Shared.IntegrationEvents
{
    /// <summary>
    /// Broker names shared by the publisher (API) and the consumer (Worker).
    /// </summary>
    public static class BookingEventsRouting
    {
        public const string Exchange = "bookings.events";
        public const string BookingConfirmedKey = "booking.confirmed.v1";
        public const string BookingCancelledKey = "booking.cancelled.v1";
    }
}
