using Ambev.DeveloperEvaluation.ORM.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

public class OutboxMessageConsumerConfiguration : IEntityTypeConfiguration<OutboxMessageConsumer>
{
    public void Configure(EntityTypeBuilder<OutboxMessageConsumer> builder)
    {
        builder.ToTable("outbox_message_consumers");

        builder.HasKey(c => new { c.OutboxMessageId, c.HandlerName });

        builder.Property(c => c.HandlerName).IsRequired().HasMaxLength(200);
    }
}
