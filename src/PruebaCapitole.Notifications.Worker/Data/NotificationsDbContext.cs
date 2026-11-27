using Microsoft.EntityFrameworkCore;

namespace PruebaCapitole.Notifications.Worker.Data
{
    public sealed class NotificationsDbContext(DbContextOptions<NotificationsDbContext> options) : DbContext(options)
    {
        public const string Schema = "notifications";

        public DbSet<Notification> Notifications => Set<Notification>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(Schema);

            modelBuilder.Entity<Notification>(builder =>
            {
                builder.ToTable("notifications");
                builder.HasKey(n => n.Id);

                builder.HasIndex(n => n.EventId).IsUnique();

                builder.Property(n => n.Type).HasMaxLength(50);
                builder.Property(n => n.ResourceId).HasMaxLength(100);
                builder.Property(n => n.CustomerEmail).HasMaxLength(320);
                builder.Property(n => n.Message).HasMaxLength(1000);
                builder.Property(n => n.CorrelationId).HasMaxLength(100);
            });
        }
    }
}
