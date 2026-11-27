using MediatR;
using PruebaCapitole.Domain.Bookings;
using PruebaCapitole.Domain.Bookings.Specifications;
using PruebaCapitole.Domain.Bookings.ValueObjects;
using PruebaCapitole.Domain.Common;
using PruebaCapitole.Service.Abstractions;
using PruebaCapitole.Service.Common;

namespace PruebaCapitole.Service.Bookings.Commands
{
    public sealed record ConfirmBookingCommand(Guid BookingId) : IRequest<Result>;

    public sealed class ConfirmBookingHandler(
        IBookingRepository repository,
        IUnitOfWork unitOfWork,
        TimeProvider clock) : IRequestHandler<ConfirmBookingCommand, Result>
    {
        public async Task<Result> Handle(ConfirmBookingCommand request, CancellationToken cancellationToken)
        {
            var booking = await repository.GetByIdAsync(new BookingId(request.BookingId), cancellationToken)
                ?? throw new DomainException(DomainErrors.NotFound, "La reserva no existe.");
            
            var overlapSpec = new ConfirmedOverlappingBookingSpec(
                booking.ResourceId, booking.DateRange, excludeId: booking.Id);

            var hasOverlap = await repository.AnyAsync(overlapSpec, cancellationToken);

            booking.Confirm(hasOverlap, clock);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
