using PruebaCapitole.Domain.Bookings;
using PruebaCapitole.Domain.Bookings.ValueObjects;
using PruebaCapitole.Domain.Common;

namespace PruebaCapitole.Service.Abstractions
{
    /// <summary>Puerto: lo que la aplicación necesita para guardar y buscar reservas.</summary>
    public interface IBookingRepository
    {
        Task<Booking?> GetByIdAsync(BookingId id, CancellationToken ct);

        Task AddAsync(Booking booking, CancellationToken ct);

        /// <summary>Evalúa la specification EN LA BASE DE DATOS (no carga reservas en memoria).</summary>
        Task<bool> AnyAsync(Specification<Booking> spec, CancellationToken ct);
    }
}
