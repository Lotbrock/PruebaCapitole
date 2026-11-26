using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PruebaCapitole.Domain.Bookings;
using PruebaCapitole.Domain.Bookings.Specifications;
using PruebaCapitole.Service.Bookings.Queries;
using PruebaCapitole.Service.Common;

namespace PruebaCapitole.Infrastructure.Data
{
    public sealed class BookingReadService(CapitoleDbContext db, IMapper mapper) : IBookingReadService
    {
        public async Task<PagedResult<BookingListItemDto>> GetPagedAsync(
            BookingsFilterSpec filter, int page, int pageSize,
            BookingSortBy sortBy, SortDirection sortDirection, CancellationToken ct)
        {
            var query = db.Set<Booking>().AsNoTracking().Where(filter.ToExpression());

            var totalCount = await query.CountAsync(ct);

            var desc = sortDirection == SortDirection.Desc;
            var ordered = sortBy switch
            {
                BookingSortBy.End => desc ? query.OrderByDescending(b => b.DateRange.End) : query.OrderBy(b => b.DateRange.End),
                BookingSortBy.CreatedAt => desc ? query.OrderByDescending(b => b.CreatedAt) : query.OrderBy(b => b.CreatedAt),
                _ => desc ? query.OrderByDescending(b => b.DateRange.Start) : query.OrderBy(b => b.DateRange.Start)
            };

            var bookings = await ordered
                .ThenBy(b => b.Id) // desempate: evita repetir u omitir filas entre páginas
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            var items = mapper.Map<List<BookingListItemDto>>(bookings);

            return new PagedResult<BookingListItemDto>(items, page, pageSize, totalCount);
        }
    }
}
