using AutoMapper;
using MediatR;
using PruebaCapitole.Domain.Bookings.ValueObjects;
using PruebaCapitole.Service.Abstractions;

namespace PruebaCapitole.Service.Bookings.Queries
{
     public sealed record GetBookingByIdQuery(Guid Id) : IRequest<BookingDetailsDto?>;

    public sealed class GetBookingByIdHandler(IBookingRepository repository, IMapper mapper)
        : IRequestHandler<GetBookingByIdQuery, BookingDetailsDto?>
    {
        public async Task<BookingDetailsDto?> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken)
        {
            if (request.Id == Guid.Empty)
                return null;

            var booking = await repository.GetByIdAsync(new BookingId(request.Id), cancellationToken);
            if (booking is null)
                return null;

            return mapper.Map<BookingDetailsDto>(booking);
        }
    }
}
