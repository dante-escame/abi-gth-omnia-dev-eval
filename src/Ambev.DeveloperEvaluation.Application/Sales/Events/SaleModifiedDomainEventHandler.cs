using Ambev.DeveloperEvaluation.Domain.Sales.Events;
using Ambev.DeveloperEvaluation.Sales.IntegrationEvents;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Ambev.DeveloperEvaluation.Application.Sales.Events;

public sealed class SaleModifiedDomainEventHandler(IBus bus, ILogger<SaleModifiedDomainEventHandler> logger)
    : INotificationHandler<SaleModifiedDomainEvent>
{
    public Task Handle(SaleModifiedDomainEvent notification, CancellationToken cancellationToken) =>
        SaleEventPublishing.PublishAsync(bus, logger, new SaleModifiedIntegrationEvent(
            notification.SaleId,
            notification.SaleNumber,
            notification.Items.Select(SoldItems.From).ToList(),
            notification.Total,
            notification.ModifiedAt,
            notification.OccurredOnUtc));
}
