using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PruebaCapitole.Infrastructure.Outbox;

namespace PruebaCapitole.Infrastructure.Data.Configurations
{
    public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(EntityTypeBuilder<OutboxMessage> builder)
        {
            builder.ToTable("OutboxMessages");
            builder.HasKey(m => m.Id);
            builder.Property(m => m.Id).ValueGeneratedNever();
            builder.Property(m => m.Type).HasMaxLength(200).IsRequired();
            builder.Property(m => m.RoutingKey).HasMaxLength(200).IsRequired();
            builder.Property(m => m.Payload).HasColumnType("jsonb").IsRequired();
            builder.Property(m => m.CorrelationId).HasMaxLength(100).IsRequired();
            builder.Property(m => m.LastError).HasMaxLength(2000);

            // The publisher only looks at pending rows.
            builder.HasIndex(m => m.OccurredAtUtc).HasFilter("\"ProcessedAtUtc\" IS NULL");
        }
    }
}
