using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PruebaCapitole.Domain.Bookings;
using PruebaCapitole.Domain.Bookings.ValueObjects;

namespace PruebaCapitole.Infrastructure.Data.Seeding
{
    /// <summary>
    /// Inserta reservas de ejemplo usando el DOMINIO (create / Confirm / Cancel),
    /// así los datos siempre cumplen las reglas de negocio.
    /// Solo actúa si está habilitado y la tabla está vacía.
    /// </summary>
    public sealed class BookingSeeder(
        CapitoleDbContext db,
        IOptions<SeedingOptions> options,
        TimeProvider clock,
        ILogger<BookingSeeder> logger)
    {
        private static readonly string[] Resources =
            ["Sala-A", "Sala-B", "Sala-C", "Puesto-1", "Puesto-2", "Vehiculo-1"];

        private static readonly string?[] CancelReasons =
            ["Cambio de planes", "Reunión cancelada", "Reserva duplicada", null];

        public async Task SeedAsync(CancellationToken ct = default)
        {
            var settings = options.Value;

            if (!settings.Enabled)
                return;

            if (await db.Bookings.AnyAsync(ct))
            {
                logger.LogInformation("Seeding omitido: ya hay reservas en la base de datos.");
                return;
            }

            Randomizer.Seed = new Random(settings.RandomSeed); // datos repetibles
            var faker = new Faker("es");

            var bookings = new List<Booking>();
            var confirmed = new List<Booking>(); // para comprobar solapes sin tocar la BD

            AddSpecialCases(bookings, confirmed, faker);
            AddRandomBookings(bookings, confirmed, faker, Math.Clamp(settings.BookingsCount, 0, 10_000));

            foreach (var booking in bookings)
                booking.ClearDomainEvents(); // aún no se despachan eventos al sembrar

            db.Bookings.AddRange(bookings);
            await db.SaveChangesAsync(ct);

            logger.LogInformation("Seeding completado: {Count} reservas insertadas.", bookings.Count);
        }

        /// <summary>Casos fijos que sirven para probar las reglas del enunciado.</summary>
        private void AddSpecialCases(List<Booking> bookings, List<Booking> confirmed, Faker faker)
        {
            var day = clock.GetUtcNow().Date.AddDays(10);
            DateTimeOffset At(int hour, int minute = 0) => new DateTimeOffset(day, TimeSpan.Zero).AddHours(hour).AddMinutes(minute);

            // 1) Confirmada 10:00-11:00
            var a = NewBooking(faker, "Sala-Demo", At(10), At(11));
            Confirm(a, confirmed);

            // 2) Adyacente 11:00-12:00: SÍ se puede confirmar ([Start, End) no se solapa)
            var b = NewBooking(faker, "Sala-Demo", At(11), At(12));
            Confirm(b, confirmed);

            // 3) Pending 10:30-11:30: solapa con una confirmada -> confirmarla debe dar booking.overlap
            var c = NewBooking(faker, "Sala-Demo", At(10, 30), At(11, 30));

            // 4) Otra Pending solapada con la anterior: las Pending sí pueden solaparse entre sí
            var d = NewBooking(faker, "Sala-Demo", At(10, 45), At(11, 15));

            // 5) Cancelada desde Confirmed, con motivo
            var e = NewBooking(faker, "Sala-Demo", At(14), At(15));
            Confirm(e, confirmed);
            e.Cancel("Motivo de ejemplo: cliente ausente", clock);
            confirmed.Remove(e);

            bookings.AddRange([a, b, c, d, e]);
        }

        private void AddRandomBookings(List<Booking> bookings, List<Booking> confirmed, Faker faker, int count)
        {
            for (var i = 0; i < count; i++)
            {
                var start = clock.GetUtcNow().Date
                    .AddDays(faker.Random.Int(1, 60))
                    .AddHours(faker.Random.Int(8, 18));
                var startOffset = new DateTimeOffset(start, TimeSpan.Zero);
                var booking = NewBooking(faker, faker.PickRandom(Resources),
                    startOffset, startOffset.AddHours(faker.Random.Int(1, 3)));

                // Reparto aproximado: 40% Pending, 30% Confirmed, 15% Cancelled (desde Pending),
                // 15% Cancelled (desde Confirmed). Si al confirmar hay solape, se queda Pending.
                var roll = faker.Random.Double();
                if (roll < 0.40)
                {
                    // se queda Pending
                }
                else if (roll < 0.70)
                {
                    Confirm(booking, confirmed);
                }
                else if (roll < 0.85)
                {
                    booking.Cancel(faker.PickRandom(CancelReasons), clock);
                }
                else if (Confirm(booking, confirmed))
                {
                    booking.Cancel(faker.PickRandom(CancelReasons), clock);
                    confirmed.Remove(booking);
                }

                bookings.Add(booking);
            }
        }

        private Booking NewBooking(Faker faker, string resource, DateTimeOffset start, DateTimeOffset end) =>
            Booking.create(
                new ResourceId(resource),
                Customer.Create(faker.Name.FullName(), faker.Internet.Email()),
                DateRange.Create(start, end),
                clock);

        /// <summary>Confirma solo si no se solapa con otra Confirmed del mismo recurso.</summary>
        private bool Confirm(Booking booking, List<Booking> confirmed)
        {
            var hasOverlap = confirmed.Any(c =>
                c.ResourceId == booking.ResourceId && c.DateRange.Overlaps(booking.DateRange));

            if (hasOverlap)
                return false;

            booking.Confirm(hasConfirmedOverlap: false, clock);
            confirmed.Add(booking);
            return true;
        }
    }
}
