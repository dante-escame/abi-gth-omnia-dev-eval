using Ambev.DeveloperEvaluation.ORM.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();

        builder.Property(m => m.Type).IsRequired().HasMaxLength(500);
        builder.Property(m => m.Payload).IsRequired().HasColumnType("jsonb");
        builder.Property(m => m.OccurredOnUtc).IsRequired();
        builder.Property(m => m.ProcessedOnUtc);
        builder.Property(m => m.Error);

        builder.HasIndex(m => new { m.ProcessedOnUtc, m.OccurredOnUtc });
    }
}
