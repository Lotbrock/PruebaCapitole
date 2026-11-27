using PruebaCapitole.Domain.Bookings;
using PruebaCapitole.Domain.Bookings.ValueObjects;

namespace PruebaCapitole.Service.Tests;

/// <summary>Clock with a fixed time, so the tests never depend on the real date.</summary>
internal sealed class TestClock(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}

internal static class TestData
{
    /// <summary>"Today" for the clock.</summary>
    public static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    /// <summary>A day in the future: At(10) is 10:00 on that day.</summary>
    private static readonly DateTimeOffset Day = new(2026, 6, 1, 0, 0, 0, TimeSpan.Zero);

    public static DateTimeOffset At(double hours) => Day.AddHours(hours);

    public static TestClock Clock() => new(Now);

    public static Booking Pending(double startHour, double endHour, string resource = "Room-A") =>
        Booking.create(
            new ResourceId(resource),
            Customer.Create("Ana Pérez", "ana@example.com"),
            DateRange.Create(At(startHour), At(endHour)),
            Clock());

    public static Booking Confirmed(double startHour, double endHour, string resource = "Room-A")
    {
        var booking = Pending(startHour, endHour, resource);
        booking.Confirm(hasConfirmedOverlap: false, Clock());
        return booking;
    }
}
