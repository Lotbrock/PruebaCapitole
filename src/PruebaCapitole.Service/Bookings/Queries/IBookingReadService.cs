using PruebaCapitole.Domain.Bookings.Specifications;
using PruebaCapitole.Service.Common;

namespace PruebaCapitole.Service.Bookings.Queries
{
    /// <summary>Puerto de lectura: lo implementa Infrastructure con EF Core.</summary>
    public interface IBookingReadService
    {
        Task<PagedResult<BookingListItemDto>> GetPagedAsync(
            BookingsFilterSpec filter, int page, int pageSize, string? sort, CancellationToken ct);
    }
}
