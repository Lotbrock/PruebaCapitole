using PruebaCapitole.Domain.Bookings.ValueObjects;
using PruebaCapitole.Service.Bookings.Commands;
using PruebaCapitole.Service.Tests.Fakes;

namespace PruebaCapitole.Service.Tests.Bookings;

public class CreateBookingHandlerTests
{
    private readonly FakeBookingRepository _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly CreateBookingHandler _sut;

    public CreateBookingHandlerTests()
    {
        _sut = new CreateBookingHandler(_repository, _unitOfWork, TestData.Clock());
    }

    private static CreateBookingCommand ValidCommand() =>
        new("Room-A", "Ana Pérez", "ana@example.com", TestData.At(10), TestData.At(11));

    [Fact]
    public async Task Handle_WithValidCommand_AddsAPendingBooking()
    {
        // Arrange
        var command = ValidCommand();

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var booking = Assert.Single(_repository.Bookings);
        Assert.Equal(BookingStatus.Pending, booking.Status);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ReturnsTheIdOfTheNewBooking()
    {
        // Arrange
        var command = ValidCommand();

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(_repository.Bookings.Single().Id.Value, result.Value);
    }

    [Fact]
    public async Task Handle_WithValidCommand_CommitsExactlyOnce()
    {
        // Arrange
        var command = ValidCommand();

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(1, _unitOfWork.SaveCount);
    }
}
