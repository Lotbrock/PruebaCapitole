using MediatR;

namespace PruebaCapitole.Service.Bookings.Commands
{
    /// <summary>F1: crear reserva en estado Pending. Devuelve el Id de la reserva.</summary>
    public sealed record CreateBookingCommand(
        string ResourceId,
        string CustomerName,
        string CustomerEmail,
        DateTimeOffset Start,
        DateTimeOffset End) : IRequest<Guid>;

    public sealed class CreateBookingHandler : IRequestHandler<CreateBookingCommand, Guid>
    {
        public Task<Guid> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
        {
            // TODO: crear el agregado (Booking.create), guardarlo y devolver su Id.
            throw new NotImplementedException();
        }
    }
}
