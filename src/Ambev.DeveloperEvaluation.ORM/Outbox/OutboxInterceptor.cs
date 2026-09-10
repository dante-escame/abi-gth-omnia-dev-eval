using System.Text.Json;
using Ambev.DeveloperEvaluation.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Ambev.DeveloperEvaluation.ORM.Outbox;

public sealed class OutboxInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Drain(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Drain(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void Drain(DbContext? context)
    {
        if (context is null)
            return;

        var aggregates = context.ChangeTracker
            .Entries<AggregateRoot>()
            .Select(entry => entry.Entity)
            .Where(aggregate => aggregate.DomainEvents.Count > 0)
            .ToList();

        foreach (var aggregate in aggregates)
        {
            foreach (var domainEvent in aggregate.DomainEvents)
            {
                context.Set<OutboxMessage>().Add(new OutboxMessage
                {
                    Id = domainEvent.Id,
                    Type = domainEvent.GetType().FullName!,
                    Payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), OutboxSerializer.Options),
                    OccurredOnUtc = domainEvent.OccurredOnUtc
                });
            }

            aggregate.ClearDomainEvents();
        }
    }
}
