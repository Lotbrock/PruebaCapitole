using AutoMapper;
using MediatR;
using PruebaCapitole.Domain.Bookings.ValueObjects;
using PruebaCapitole.Domain.Bookings;
using PruebaCapitole.Service.Abstractions;
using PruebaCapitole.Service.Common;

namespace PruebaCapitole.Service.Bookings.Queries
{
     public sealed record GetBookingByIdQuery(Guid Id) : IRequest<Result<BookingDetailsDto>>;

    public sealed class GetBookingByIdHandler(IBookingRepository repository, IMapper mapper)
        : IRequestHandler<GetBookingByIdQuery, Result<BookingDetailsDto>>
    {
        public async Task<Result<BookingDetailsDto>> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken)
        {
            var booking = request.Id == Guid.Empty
                ? null
                : await repository.GetByIdAsync(new BookingId(request.Id), cancellationToken);

            if (booking is null)
                return Result<BookingDetailsDto>.Failure(new Error(DomainErrors.NotFound, "La reserva no existe."));

            return Result<BookingDetailsDto>.Success(mapper.Map<BookingDetailsDto>(booking));
        }
    }
}
