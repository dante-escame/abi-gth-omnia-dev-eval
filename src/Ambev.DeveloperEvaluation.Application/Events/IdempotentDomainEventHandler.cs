using Ambev.DeveloperEvaluation.Application.Ports;
using Ambev.DeveloperEvaluation.Domain.Common;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Events;

public sealed class IdempotentDomainEventHandler<TDomainEvent>(
    INotificationHandler<TDomainEvent> inner,
    IOutboxConsumerTracker tracker)
    : INotificationHandler<TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    public async Task Handle(TDomainEvent notification, CancellationToken cancellationToken)
    {
        string handlerName = inner.GetType().Name;

        if (await tracker.HasRunAsync(notification.Id, handlerName, cancellationToken))
            return;

        await inner.Handle(notification, cancellationToken);

        await tracker.MarkAsRunAsync(notification.Id, handlerName, cancellationToken);
    }
}
