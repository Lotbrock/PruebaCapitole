using MediatR;
using PruebaCapitole.Domain.Bookings;
using PruebaCapitole.Domain.Bookings.Specifications;
using PruebaCapitole.Domain.Bookings.ValueObjects;
using PruebaCapitole.Domain.Common;
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
        BookingSortBy? SortBy,
        SortDirection? SortDirection) : IRequest<Result<PagedResult<BookingListItemDto>>>;

    public sealed class GetBookingsHandler(IBookingReadService readService)
        : IRequestHandler<GetBookingsQuery, Result<PagedResult<BookingListItemDto>>>
    {
        private const int DefaultPageSize = 20;
        private const int MaxPageSize = 100;

        public async Task<Result<PagedResult<BookingListItemDto>>> Handle(GetBookingsQuery request, CancellationToken cancellationToken)
        {
            if (request.From >= request.To)
                throw new DomainException(DomainErrors.InvalidRange, "'from' debe ser anterior a 'to'.");

            ResourceId? resourceId = string.IsNullOrWhiteSpace(request.ResourceId)
                ? null
                : new ResourceId(request.ResourceId);

            var filter = new BookingsFilterSpec(resourceId, request.From, request.To);

            var page = Math.Max(request.Page ?? 1, 1);
            var pageSize = Math.Clamp(request.PageSize ?? DefaultPageSize, 1, MaxPageSize);

            var sortBy = request.SortBy ?? BookingSortBy.Start;
            var sortDirection = request.SortDirection ?? SortDirection.Asc;

            var result = await readService.GetPagedAsync(filter, page, pageSize, sortBy, sortDirection, cancellationToken);
            return Result<PagedResult<BookingListItemDto>>.Success(result);
        }
    }
}
