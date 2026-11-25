using MediatR;

namespace PruebaCapitole.Service.Bookings.Commands
{
    /// <summary>F4: cancelar una reserva Pending o Confirmed, con motivo opcional.</summary>
    public sealed record CancelBookingCommand(Guid BookingId, string? Reason) : IRequest;

    public sealed class CancelBookingHandler : IRequestHandler<CancelBookingCommand>
    {
        public Task Handle(CancelBookingCommand request, CancellationToken cancellationToken)
        {
            // TODO: cargar el agregado, Cancel(reason) y guardar.
            throw new NotImplementedException();
        }
    }
}
