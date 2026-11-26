using Microsoft.EntityFrameworkCore;
using PruebaCapitole.Domain.Bookings;
using PruebaCapitole.Domain.Bookings.Specifications;
using PruebaCapitole.Service.Bookings.Queries;
using PruebaCapitole.Service.Common;

namespace PruebaCapitole.Infrastructure.Data
{
    public sealed class BookingReadService(CapitoleDbContext db) : IBookingReadService
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

            var items = await ordered
                .ThenBy(b => b.Id) // desempate: evita repetir u omitir filas entre páginas
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BookingListItemDto(
                    b.Id.Value,
                    b.ResourceId.Value,
                    b.Customer.Name,
                    b.Customer.Email,
                    b.DateRange.Start,
                    b.DateRange.End,
                    b.Status.ToString(),
                    b.CreatedAt))
                .ToListAsync(ct);

            return new PagedResult<BookingListItemDto>(items, page, pageSize, totalCount);
        }
    }
}
