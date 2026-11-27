using PruebaCapitole.Domain.Bookings;
using PruebaCapitole.Domain.Bookings.ValueObjects;
using PruebaCapitole.Domain.Common;
using PruebaCapitole.Service.Bookings.Commands;
using PruebaCapitole.Service.Tests.Fakes;

namespace PruebaCapitole.Service.Tests.Bookings;

public class ConfirmBookingHandlerTests
{
    private readonly FakeBookingRepository _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly ConfirmBookingHandler _sut;

    public ConfirmBookingHandlerTests()
    {
        _sut = new ConfirmBookingHandler(_repository, _unitOfWork, TestData.Clock());
    }

    [Fact]
    public async Task Handle_WithoutOverlap_ConfirmsTheBooking()
    {
        // Arrange
        var booking = TestData.Pending(10, 11);
        _repository.Bookings.Add(booking);
        var command = new ConfirmBookingCommand(booking.Id.Value);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(BookingStatus.Confirmed, booking.Status);
    }

    [Fact]
    public async Task Handle_WithoutOverlap_CommitsExactlyOnce()
    {
        // Arrange
        var booking = TestData.Pending(10, 11);
        _repository.Bookings.Add(booking);
        var command = new ConfirmBookingCommand(booking.Id.Value);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_WithOverlappingConfirmedBooking_ThrowsOverlap()
    {
        // Arrange
        _repository.Bookings.Add(TestData.Confirmed(10, 11));
        var candidate = TestData.Pending(10.5, 11.5);
        _repository.Bookings.Add(candidate);
        var command = new ConfirmBookingCommand(candidate.Id.Value);

        // Act
        var exception = await Assert.ThrowsAsync<DomainException>(
            () => _sut.Handle(command, CancellationToken.None));

        // Assert
        Assert.Equal(DomainErrors.Overlap, exception.Code);
    }

    [Fact]
    public async Task Handle_WithOverlappingConfirmedBooking_KeepsPendingAndDoesNotCommit()
    {
        // Arrange
        _repository.Bookings.Add(TestData.Confirmed(10, 11));
        var candidate = TestData.Pending(10.5, 11.5);
        _repository.Bookings.Add(candidate);
        var command = new ConfirmBookingCommand(candidate.Id.Value);

        // Act
        await Assert.ThrowsAsync<DomainException>(() => _sut.Handle(command, CancellationToken.None));

        // Assert
        Assert.Equal(BookingStatus.Pending, candidate.Status);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_WithAdjacentConfirmedBooking_ConfirmsTheBooking()
    {
        // Arrange
        _repository.Bookings.Add(TestData.Confirmed(10, 11));
        var candidate = TestData.Pending(11, 12); // starts exactly when the other one ends
        _repository.Bookings.Add(candidate);
        var command = new ConfirmBookingCommand(candidate.Id.Value);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(BookingStatus.Confirmed, candidate.Status);
    }

    [Fact]
    public async Task Handle_WithConfirmedBookingOfAnotherResource_ConfirmsTheBooking()
    {
        // Arrange
        _repository.Bookings.Add(TestData.Confirmed(10, 11, resource: "Room-B"));
        var candidate = TestData.Pending(10, 11, resource: "Room-A");
        _repository.Bookings.Add(candidate);
        var command = new ConfirmBookingCommand(candidate.Id.Value);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_WithUnknownBooking_ThrowsNotFoundAndDoesNotCommit()
    {
        // Arrange
        var command = new ConfirmBookingCommand(Guid.NewGuid());

        // Act
        var exception = await Assert.ThrowsAsync<DomainException>(
            () => _sut.Handle(command, CancellationToken.None));

        // Assert
        Assert.Equal(DomainErrors.NotFound, exception.Code);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }
}
