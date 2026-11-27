using Microsoft.EntityFrameworkCore;
using PruebaCapitole.Domain.Bookings;
using PruebaCapitole.Infrastructure.Outbox;
using PruebaCapitole.Service.Abstractions;

namespace PruebaCapitole.Infrastructure.Data
{
    /// <summary>
    /// DbContext de la aplicación. También hace de Unit of Work: el SaveChangesAsync
    /// que ya trae EF Core cumple el puerto IUnitOfWork.
    /// </summary>
    public class CapitoleDbContext(DbContextOptions<CapitoleDbContext> options)
        : DbContext(options), IUnitOfWork
    {
        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Busca todas las clases IEntityTypeConfiguration<T> de este proyecto.
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CapitoleDbContext).Assembly);
        }
    }
}
