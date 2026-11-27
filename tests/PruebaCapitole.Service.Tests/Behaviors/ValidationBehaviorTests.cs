using FluentValidation;
using MediatR;
using PruebaCapitole.Service.Behaviors;
using PruebaCapitole.Service.Bookings.Commands;
using PruebaCapitole.Service.Bookings.Validators;
using PruebaCapitole.Service.Common;

namespace PruebaCapitole.Service.Tests.Behaviors;

public class ValidationBehaviorTests
{
    private readonly ValidationBehavior<CreateBookingCommand, Result<Guid>> _sut =
        new([new CreateBookingValidator(TestData.Clock())]);

    private static CreateBookingCommand ValidCommand() =>
        new("Room-A", "Ana Pérez", "ana@example.com", TestData.At(10), TestData.At(11));

    /// <summary>Stands for the handler: records whether it was reached.</summary>
    private sealed class NextSpy(Result<Guid> response)
    {
        public bool WasCalled { get; private set; }

        public Task<Result<Guid>> Invoke(CancellationToken cancellationToken)
        {
            WasCalled = true;
            return Task.FromResult(response);
        }
    }

    [Fact]
    public async Task Handle_WithValidCommand_CallsTheHandler()
    {
        // Arrange
        var next = new NextSpy(Result<Guid>.Success(Guid.NewGuid()));

        // Act
        await _sut.Handle(ValidCommand(), next.Invoke, CancellationToken.None);

        // Assert
        Assert.True(next.WasCalled);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ReturnsTheHandlerResult()
    {
        // Arrange
        var expected = Result<Guid>.Success(Guid.NewGuid());
        var next = new NextSpy(expected);

        // Act
        var result = await _sut.Handle(ValidCommand(), next.Invoke, CancellationToken.None);

        // Assert
        Assert.Same(expected, result);
    }

    [Fact]
    public async Task Handle_WithInvalidNameAndEmail_ReturnsValidationErrorWithBothFields()
    {
        // Arrange
        var command = ValidCommand() with { CustomerName = "", CustomerEmail = "not-an-email" };
        var next = new NextSpy(Result<Guid>.Success(Guid.NewGuid()));

        // Act
        var result = await _sut.Handle(command, next.Invoke, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(Error.ValidationCode, result.Error!.Code);
        Assert.Contains("CustomerName", result.Error.Details!.Keys);
        Assert.Contains("CustomerEmail", result.Error.Details.Keys);
    }

    [Fact]
    public async Task Handle_WithInvalidCommand_DoesNotCallTheHandler()
    {
        // Arrange
        var command = ValidCommand() with { CustomerName = "" };
        var next = new NextSpy(Result<Guid>.Success(Guid.NewGuid()));

        // Act
        await _sut.Handle(command, next.Invoke, CancellationToken.None);

        // Assert
        Assert.False(next.WasCalled);
    }

    [Fact]
    public async Task Handle_WithStartInThePast_ReportsTheStartField()
    {
        // Arrange
        var pastStart = TestData.Now.AddDays(-1);
        var command = ValidCommand() with { Start = pastStart, End = pastStart.AddHours(1) };
        var next = new NextSpy(Result<Guid>.Success(Guid.NewGuid()));

        // Act
        var result = await _sut.Handle(command, next.Invoke, CancellationToken.None);

        // Assert
        Assert.Contains("Start", result.Error!.Details!.Keys);
    }

    [Fact]
    public async Task Handle_WithEndBeforeStart_ReportsTheEndField()
    {
        // Arrange
        var command = ValidCommand() with { Start = TestData.At(11), End = TestData.At(10) };
        var next = new NextSpy(Result<Guid>.Success(Guid.NewGuid()));

        // Act
        var result = await _sut.Handle(command, next.Invoke, CancellationToken.None);

        // Assert
        Assert.Contains("End", result.Error!.Details!.Keys);
    }

    [Fact]
    public async Task Handle_WithoutValidators_CallsTheHandler()
    {
        // Arrange
        var sut = new ValidationBehavior<CreateBookingCommand, Result<Guid>>(
            Array.Empty<IValidator<CreateBookingCommand>>());
        var next = new NextSpy(Result<Guid>.Success(Guid.NewGuid()));

        // Act
        await sut.Handle(ValidCommand() with { CustomerName = "" }, next.Invoke, CancellationToken.None);

        // Assert
        Assert.True(next.WasCalled);
    }
}
