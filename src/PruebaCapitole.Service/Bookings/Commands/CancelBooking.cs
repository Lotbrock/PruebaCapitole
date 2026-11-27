using MediatR;
using PruebaCapitole.Domain.Bookings;
using PruebaCapitole.Domain.Bookings.ValueObjects;
using PruebaCapitole.Domain.Common;
using PruebaCapitole.Service.Abstractions;
using PruebaCapitole.Service.Common;

namespace PruebaCapitole.Service.Bookings.Commands
{
    public sealed record CancelBookingCommand(Guid BookingId, string? Reason) : IRequest<Result>;

    public sealed class CancelBookingHandler(
        IBookingRepository repository,
        IUnitOfWork unitOfWork,
        TimeProvider clock) : IRequestHandler<CancelBookingCommand, Result>
    {
        public async Task<Result> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
        {
            var booking = await repository.GetByIdAsync(new BookingId(request.BookingId), cancellationToken)
                ?? throw new DomainException(DomainErrors.NotFound, "La reserva no existe.");

            booking.Cancel(request.Reason, clock);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
