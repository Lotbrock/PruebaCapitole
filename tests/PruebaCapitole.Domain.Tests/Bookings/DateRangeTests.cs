using PruebaCapitole.Domain.Bookings;
using PruebaCapitole.Domain.Bookings.ValueObjects;
using PruebaCapitole.Domain.Common;

namespace PruebaCapitole.Domain.Tests.Bookings;

public class DateRangeTests
{
    [Theory]
    [InlineData(11, 10)] // end before start
    [InlineData(10, 10)] // empty range
    public void Create_WhenStartIsNotBeforeEnd_ThrowsInvalidRange(double startHour, double endHour)
    {
        // Arrange
        var start = TestData.At(startHour);
        var end = TestData.At(endHour);

        // Act
        var exception = Assert.Throws<DomainException>(() => DateRange.Create(start, end));

        // Assert
        Assert.Equal(DomainErrors.InvalidRange, exception.Code);
    }

    [Fact]
    public void Create_WithOffsetDates_StoresThemInUtc()
    {
        // Arrange
        var start = new DateTimeOffset(2026, 6, 1, 12, 0, 0, TimeSpan.FromHours(2)); // 10:00 UTC
        var end = start.AddHours(1);

        // Act
        var range = DateRange.Create(start, end);

        // Assert
        Assert.Equal(TimeSpan.Zero, range.Start.Offset);
        Assert.Equal(TestData.At(10), range.Start);
    }

    // Intervals are half-open [Start, End): 10:00-11:00 and 11:00-12:00 do NOT overlap.
    [Theory]
    [InlineData(10, 11, 10.5, 11.5, true)]  // partial overlap
    [InlineData(10.5, 11.5, 10, 11, true)]  // partial overlap, reversed
    [InlineData(10, 12, 10.5, 11, true)]    // one contains the other
    [InlineData(10, 11, 10, 11, true)]      // identical
    [InlineData(10, 11, 11, 12, false)]     // adjacent: the end touches the start
    [InlineData(11, 12, 10, 11, false)]     // adjacent, reversed
    [InlineData(10, 11, 12, 13, false)]     // separated
    public void Overlaps_WithAnotherRange_UsesHalfOpenIntervals(
        double startA, double endA, double startB, double endB, bool expected)
    {
        // Arrange
        var rangeA = TestData.Range(startA, endA);
        var rangeB = TestData.Range(startB, endB);

        // Act
        var aOverlapsB = rangeA.Overlaps(rangeB);
        var bOverlapsA = rangeB.Overlaps(rangeA);

        // Assert
        Assert.Equal(expected, aOverlapsB);
        Assert.Equal(expected, bOverlapsA);
    }

    [Fact]
    public void Equals_WithSameDates_ReturnsTrue()
    {
        // Arrange
        var first = TestData.Range(10, 11);
        var second = TestData.Range(10, 11);

        // Act
        var areEqual = first == second;

        // Assert
        Assert.True(areEqual);
    }
}
