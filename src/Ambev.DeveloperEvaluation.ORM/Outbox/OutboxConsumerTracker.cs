using Ambev.DeveloperEvaluation.Application.Ports;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Outbox;

public sealed class OutboxConsumerTracker(DefaultContext context) : IOutboxConsumerTracker
{
    public Task<bool> HasRunAsync(Guid outboxMessageId, string handlerName, CancellationToken cancellationToken = default)
    {
        return context.OutboxMessageConsumers
            .AsNoTracking()
            .AnyAsync(c => 
                c.OutboxMessageId == outboxMessageId 
                && c.HandlerName == handlerName, cancellationToken);
    }

    public async Task MarkAsRunAsync(Guid outboxMessageId, string handlerName, CancellationToken cancellationToken = default)
    {
        context.OutboxMessageConsumers.Add(new OutboxMessageConsumer
        {
            OutboxMessageId = outboxMessageId,
            HandlerName = handlerName
        });

        await context.SaveChangesAsync(cancellationToken);
    }
}
