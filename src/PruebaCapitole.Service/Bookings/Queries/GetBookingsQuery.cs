using MediatR;
using PruebaCapitole.Service.Common;

namespace PruebaCapitole.Service.Bookings.Queries
{
    /// <summary>F2: listado filtrado por recurso y rango de fechas, con paginación y orden en servidor.</summary>
    public sealed record GetBookingsQuery(
        string? ResourceId,
        DateTimeOffset? From,
        DateTimeOffset? To,
        int? Page,
        int? PageSize,
        string? Sort) : IRequest<PagedResult<BookingListItemDto>>;

    public sealed class GetBookingsHandler : IRequestHandler<GetBookingsQuery, PagedResult<BookingListItemDto>>
    {
        public Task<PagedResult<BookingListItemDto>> Handle(GetBookingsQuery request, CancellationToken cancellationToken)
        {
            // TODO: validar from < to, normalizar page/pageSize, crear BookingsFilterSpec
            //       y llamar a IBookingReadService.GetPagedAsync(...).
            throw new NotImplementedException();
        }
    }
}
