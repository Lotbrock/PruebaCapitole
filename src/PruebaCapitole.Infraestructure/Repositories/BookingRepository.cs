using Microsoft.EntityFrameworkCore;
using PruebaCapitole.Domain.Bookings;
using PruebaCapitole.Domain.Bookings.ValueObjects;
using PruebaCapitole.Domain.Common;
using PruebaCapitole.Infrastructure.Data;
using PruebaCapitole.Service.Abstractions;

namespace PruebaCapitole.Infrastructure.Repositories
{
    public sealed class BookingRepository(CapitoleDbContext db) : IBookingRepository
    {
        public Task<Booking?> GetByIdAsync(BookingId id, CancellationToken ct) =>
            db.Bookings.FirstOrDefaultAsync(b => b.Id == id, ct);

        public async Task AddAsync(Booking booking, CancellationToken ct) =>
            await db.Bookings.AddAsync(booking, ct);

        public Task<bool> AnyAsync(Specification<Booking> spec, CancellationToken ct) =>
            db.Bookings.AnyAsync(spec.ToExpression(), ct);
    }
}
