using MediatR;

namespace PruebaCapitole.Service.Bookings.Queries
{
    /// <summary>F5: detalle de una reserva. Devuelve null si no existe (la API responde 404).</summary>
    public sealed record GetBookingByIdQuery(Guid Id) : IRequest<BookingDetailsDto?>;

    public sealed class GetBookingByIdHandler : IRequestHandler<GetBookingByIdQuery, BookingDetailsDto?>
    {
        public Task<BookingDetailsDto?> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken)
        {
            // TODO: leer con AsNoTracking y proyectar a BookingDetailsDto.
            throw new NotImplementedException();
        }
    }
}
