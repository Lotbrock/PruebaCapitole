using PruebaCapitole.Domain.Bookings;
using PruebaCapitole.Domain.Common;
using PruebaCapitole.Service.Behaviors;
using PruebaCapitole.Service.Bookings.Commands;
using PruebaCapitole.Service.Common;

namespace PruebaCapitole.Service.Tests.Behaviors;

public class ExceptionHandlingBehaviorTests
{
    private readonly ExceptionHandlingBehavior<ConfirmBookingCommand, Result> _sut = new();
    private readonly ConfirmBookingCommand _command = new(Guid.NewGuid());

    [Fact]
    public async Task Handle_WhenHandlerThrowsDomainException_ReturnsFailedResultWithItsCode()
    {
        // Arrange
        var exception = new DomainException(DomainErrors.Overlap, "Hay solape.");

        // Act
        var result = await _sut.Handle(
            _command, _ => Task.FromException<Result>(exception), CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrors.Overlap, result.Error!.Code);
    }

    [Fact]
    public async Task Handle_WhenHandlerThrowsDomainException_ReturnsFailedResultWithItsMessage()
    {
        // Arrange
        var exception = new DomainException(DomainErrors.Overlap, "Hay solape.");

        // Act
        var result = await _sut.Handle(
            _command, _ => Task.FromException<Result>(exception), CancellationToken.None);

        // Assert
        Assert.Equal("Hay solape.", result.Error!.Message);
    }

    [Fact]
    public async Task Handle_WhenHandlerSucceeds_ReturnsItsResult()
    {
        // Arrange
        var expected = Result.Success();

        // Act
        var result = await _sut.Handle(_command, _ => Task.FromResult(expected), CancellationToken.None);

        // Assert
        Assert.Same(expected, result);
    }

    [Fact]
    public async Task Handle_WhenHandlerThrowsUnexpectedException_DoesNotHideIt()
    {
        // Arrange
        var exception = new InvalidOperationException("boom");

        // Act
        var act = () => _sut.Handle(
            _command, _ => Task.FromException<Result>(exception), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(act);
    }
}
