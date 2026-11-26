using MediatR;
using PruebaCapitole.Domain.Bookings.ValueObjects;
using PruebaCapitole.Service.Abstractions;

namespace PruebaCapitole.Service.Bookings.Queries
{
    /// <summary>F5: detalle de una reserva. Devuelve null si no existe (la API responde 404).</summary>
    public sealed record GetBookingByIdQuery(Guid Id) : IRequest<BookingDetailsDto?>;

    public sealed class GetBookingByIdHandler(IBookingRepository repository)
        : IRequestHandler<GetBookingByIdQuery, BookingDetailsDto?>
    {
        public async Task<BookingDetailsDto?> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken)
        {
            if (request.Id == Guid.Empty)
                return null;

            var booking = await repository.GetByIdAsync(new BookingId(request.Id), cancellationToken);
            if (booking is null)
                return null;

            return new BookingDetailsDto(
                booking.Id.Value,
                booking.ResourceId.Value,
                booking.Customer.Name,
                booking.Customer.Email,
                booking.DateRange.Start,
                booking.DateRange.End,
                booking.Status.ToString(),
                booking.CreatedAt,
                booking.ConfirmedAt,
                booking.CancelledAt,
                booking.CancellationReason);
        }
    }
}
