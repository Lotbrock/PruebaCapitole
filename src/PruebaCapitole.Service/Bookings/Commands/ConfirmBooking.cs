using MediatR;

namespace PruebaCapitole.Service.Bookings.Commands
{
    /// <summary>F3: confirmar una reserva Pending (bloquea solapes con otras Confirmed).</summary>
    public sealed record ConfirmBookingCommand(Guid BookingId) : IRequest;

    public sealed class ConfirmBookingHandler : IRequestHandler<ConfirmBookingCommand>
    {
        public Task Handle(ConfirmBookingCommand request, CancellationToken cancellationToken)
        {
            // TODO: cargar el agregado, evaluar ConfirmedOverlappingBookingSpec, Confirm(...) y guardar.
            throw new NotImplementedException();
        }
    }
}
