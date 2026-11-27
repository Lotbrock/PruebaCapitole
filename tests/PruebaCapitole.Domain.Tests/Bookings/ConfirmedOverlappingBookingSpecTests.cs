using PruebaCapitole.Domain.Bookings;
using PruebaCapitole.Domain.Bookings.Specifications;
using PruebaCapitole.Domain.Bookings.ValueObjects;

namespace PruebaCapitole.Domain.Tests.Bookings;

/// <summary>
/// The specification is an expression (EF Core translates it to SQL).
/// Here it is compiled and run in memory only to check the rule itself.
/// </summary>
public class ConfirmedOverlappingBookingSpecTests
{
    private static bool Evaluate(
        Booking existing, string resource, double startHour, double endHour, BookingId? excludeId = null)
    {
        var spec = new ConfirmedOverlappingBookingSpec(
            new ResourceId(resource), TestData.Range(startHour, endHour), excludeId);

        return spec.ToExpression().Compile()(existing);
    }

    [Fact]
    public void ToExpression_ConfirmedBookingThatOverlaps_Matches()
    {
        // Arrange
        var existing = TestData.Confirmed(10, 11);

        // Act
        var matches = Evaluate(existing, "Room-A", 10.5, 11.5);

        // Assert
        Assert.True(matches);
    }

    [Fact]
    public void ToExpression_AdjacentRange_DoesNotMatch()
    {
        // Arrange
        var existing = TestData.Confirmed(10, 11);

        // Act
        var matches = Evaluate(existing, "Room-A", 11, 12);

        // Assert
        Assert.False(matches);
    }

    [Fact]
    public void ToExpression_DifferentResource_DoesNotMatch()
    {
        // Arrange
        var existing = TestData.Confirmed(10, 11, resource: "Room-A");

        // Act
        var matches = Evaluate(existing, "Room-B", 10, 11);

        // Assert
        Assert.False(matches);
    }

    [Fact]
    public void ToExpression_PendingBooking_DoesNotMatchBecauseOnlyConfirmedOnesBlock()
    {
        // Arrange
        var existing = TestData.Pending(10, 11);

        // Act
        var matches = Evaluate(existing, "Room-A", 10, 11);

        // Assert
        Assert.False(matches);
    }

    [Fact]
    public void ToExpression_ExcludedBooking_DoesNotMatchItself()
    {
        // Arrange
        var existing = TestData.Confirmed(10, 11);

        // Act
        var matches = Evaluate(existing, "Room-A", 10, 11, excludeId: existing.Id);

        // Assert
        Assert.False(matches);
    }
}
