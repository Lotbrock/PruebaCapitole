using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PruebaCapitole.Domain.Bookings;
using PruebaCapitole.Domain.Bookings.ValueObjects;

namespace PruebaCapitole.Infrastructure.Data.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.ToTable("bookings");

            // Ids con tipo fuerte: el dominio usa BookingId/ResourceId, la BD guarda Guid/string.
            builder.HasKey(b => b.Id);
            builder.Property(b => b.Id)
                .HasConversion(id => id.Value, value => new BookingId(value))
                .ValueGeneratedNever();

            builder.Property(b => b.ResourceId)
                .HasConversion(id => id.Value, value => new ResourceId(value))
                .HasMaxLength(100)
                .IsRequired();

            // Value objects con varias propiedades: columnas dentro de la misma tabla.
            builder.ComplexProperty(b => b.Customer, c =>
            {
                c.Property(p => p.Name).HasColumnName("customer_name").HasMaxLength(100).IsRequired();
                c.Property(p => p.Email).HasColumnName("customer_email").HasMaxLength(320).IsRequired();
            });

            builder.ComplexProperty(b => b.DateRange, r =>
            {
                r.Property(p => p.Start).HasColumnName("start_utc");
                r.Property(p => p.End).HasColumnName("end_utc");
            });

            builder.Property(b => b.Status).HasConversion<string>().HasMaxLength(20);
            builder.Property(b => b.CancellationReason).HasMaxLength(Booking.ReasonMaxLength);

            // Los eventos de dominio viven solo en memoria.
            builder.Ignore(b => b.DomainEvents);

            // Concurrencia optimista con la columna de sistema xmin de PostgreSQL.
            builder.Property<uint>("xmin").HasColumnType("xid").ValueGeneratedOnAddOrUpdate().IsConcurrencyToken();

            builder.HasIndex(b => new { b.ResourceId, b.Status });
        }
    }
}
