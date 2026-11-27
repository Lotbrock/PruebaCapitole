using PruebaCapitole.Domain.Bookings;
using PruebaCapitole.Domain.Bookings.ValueObjects;
using PruebaCapitole.Domain.Common;
using PruebaCapitole.Service.Abstractions;

namespace PruebaCapitole.Service.Tests.Fakes;

/// <summary>
/// In-memory repository. It runs the specification expression over a list,
/// which is enough to test the handlers without a database.
/// </summary>
internal sealed class FakeBookingRepository : IBookingRepository
{
    public List<Booking> Bookings { get; } = [];

    public Task<Booking?> GetByIdAsync(BookingId id, CancellationToken ct) =>
        Task.FromResult(Bookings.FirstOrDefault(b => b.Id == id));

    public Task AddAsync(Booking booking, CancellationToken ct)
    {
        Bookings.Add(booking);
        return Task.CompletedTask;
    }

    public Task<bool> AnyAsync(Specification<Booking> spec, CancellationToken ct) =>
        Task.FromResult(Bookings.Any(spec.ToExpression().Compile()));
}

/// <summary>Counts how many times the use case committed.</summary>
internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        SaveCount++;
        return Task.FromResult(1);
    }
}
