using PruebaCapitole.Domain.Bookings;
using PruebaCapitole.Domain.Bookings.Events;
using PruebaCapitole.Domain.Bookings.ValueObjects;
using PruebaCapitole.Domain.Common;

namespace PruebaCapitole.Domain.Tests.Bookings;

public class BookingTests
{
    // ---------- Create ----------

    [Fact]
    public void Create_WithFutureStart_ReturnsPendingBookingAndRaisesBookingCreated()
    {
        // Arrange
        var clock = TestData.Clock();

        // Act
        var booking = TestData.Pending(10, 11);

        // Assert
        Assert.Equal(BookingStatus.Pending, booking.Status);
        Assert.Equal(clock.GetUtcNow(), booking.CreatedAt);
        Assert.Single(booking.DomainEvents.OfType<BookingCreated>());
    }

    [Fact]
    public void Create_WithStartInThePast_ThrowsStartInPast()
    {
        // Arrange
        var pastRange = DateRange.Create(TestData.Now.AddDays(-1), TestData.Now.AddDays(-1).AddHours(1));

        // Act
        var exception = Assert.Throws<DomainException>(() =>
            Booking.create(new ResourceId("Room-A"), TestData.Ana(), pastRange, TestData.Clock()));

        // Assert
        Assert.Equal(DomainErrors.StartInPast, exception.Code);
    }

    // ---------- Confirm ----------

    [Fact]
    public void Confirm_PendingBookingWithoutOverlap_BecomesConfirmed()
    {
        // Arrange
        var booking = TestData.Pending(10, 11);

        // Act
        booking.Confirm(hasConfirmedOverlap: false, TestData.Clock());

        // Assert
        Assert.Equal(BookingStatus.Confirmed, booking.Status);
        Assert.Equal(TestData.Now, booking.ConfirmedAt);
    }

    [Fact]
    public void Confirm_PendingBookingWithoutOverlap_RaisesBookingConfirmed()
    {
        // Arrange
        var booking = TestData.Pending(10, 11);

        // Act
        booking.Confirm(hasConfirmedOverlap: false, TestData.Clock());

        // Assert
        Assert.Single(booking.DomainEvents.OfType<BookingConfirmed>());
    }

    [Fact]
    public void Confirm_WithConfirmedOverlap_ThrowsOverlap()
    {
        // Arrange
        var booking = TestData.Pending(10, 11);

        // Act
        var exception = Assert.Throws<DomainException>(() =>
            booking.Confirm(hasConfirmedOverlap: true, TestData.Clock()));

        // Assert
        Assert.Equal(DomainErrors.Overlap, exception.Code);
    }

    [Fact]
    public void Confirm_WithConfirmedOverlap_KeepsStateAndRaisesNoEvent()
    {
        // Arrange
        var booking = TestData.Pending(10, 11);

        // Act
        Assert.Throws<DomainException>(() => booking.Confirm(hasConfirmedOverlap: true, TestData.Clock()));

        // Assert
        Assert.Equal(BookingStatus.Pending, booking.Status);
        Assert.Null(booking.ConfirmedAt);
        Assert.Empty(booking.DomainEvents.OfType<BookingConfirmed>());
    }

    [Fact]
    public void Confirm_AlreadyConfirmedBooking_ThrowsInvalidTransition()
    {
        // Arrange
        var booking = TestData.Confirmed(10, 11);

        // Act
        var exception = Assert.Throws<DomainException>(() => booking.Confirm(false, TestData.Clock()));

        // Assert
        Assert.Equal(DomainErrors.InvalidTransition, exception.Code);
    }

    [Fact]
    public void Confirm_CancelledBooking_ThrowsInvalidTransition()
    {
        // Arrange
        var booking = TestData.Pending(10, 11);
        booking.Cancel(null, TestData.Clock());

        // Act
        var exception = Assert.Throws<DomainException>(() => booking.Confirm(false, TestData.Clock()));

        // Assert
        Assert.Equal(DomainErrors.InvalidTransition, exception.Code);
    }

    // ---------- Cancel ----------

    [Fact]
    public void Cancel_PendingBooking_BecomesCancelledWithTrimmedReason()
    {
        // Arrange
        var booking = TestData.Pending(10, 11);

        // Act
        booking.Cancel("  Cambio de planes  ", TestData.Clock());

        // Assert
        Assert.Equal(BookingStatus.Cancelled, booking.Status);
        Assert.Equal(TestData.Now, booking.CancelledAt);
        Assert.Equal("Cambio de planes", booking.CancellationReason);
    }

    [Fact]
    public void Cancel_PendingBooking_RaisesBookingCancelledWithTheReason()
    {
        // Arrange
        var booking = TestData.Pending(10, 11);

        // Act
        booking.Cancel("Cambio de planes", TestData.Clock());

        // Assert
        var raised = Assert.Single(booking.DomainEvents.OfType<BookingCancelled>());
        Assert.Equal("Cambio de planes", raised.Reason);
    }

    [Fact]
    public void Cancel_ConfirmedBooking_BecomesCancelled()
    {
        // Arrange
        var booking = TestData.Confirmed(10, 11);

        // Act
        booking.Cancel(null, TestData.Clock());

        // Assert
        Assert.Equal(BookingStatus.Cancelled, booking.Status);
    }

    [Fact]
    public void Cancel_AlreadyCancelledBooking_ThrowsInvalidTransition()
    {
        // Arrange
        var booking = TestData.Pending(10, 11);
        booking.Cancel(null, TestData.Clock());

        // Act
        var exception = Assert.Throws<DomainException>(() => booking.Cancel(null, TestData.Clock()));

        // Assert
        Assert.Equal(DomainErrors.InvalidTransition, exception.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Cancel_WithBlankReason_StoresNoReason(string? reason)
    {
        // Arrange
        var booking = TestData.Pending(10, 11);

        // Act
        booking.Cancel(reason, TestData.Clock());

        // Assert
        Assert.Null(booking.CancellationReason);
    }

    [Fact]
    public void Cancel_WithReasonLongerThanMaximum_CutsItToTheMaximum()
    {
        // Arrange
        var booking = TestData.Pending(10, 11);
        var longReason = new string('x', Booking.ReasonMaxLength + 100);

        // Act
        booking.Cancel(longReason, TestData.Clock());

        // Assert
        Assert.Equal(Booking.ReasonMaxLength, booking.CancellationReason!.Length);
    }

    // ---------- Domain events ----------

    [Fact]
    public void ClearDomainEvents_AfterCreate_LeavesNoEvents()
    {
        // Arrange
        var booking = TestData.Pending(10, 11);

        // Act
        booking.ClearDomainEvents();

        // Assert
        Assert.Empty(booking.DomainEvents);
    }
}
