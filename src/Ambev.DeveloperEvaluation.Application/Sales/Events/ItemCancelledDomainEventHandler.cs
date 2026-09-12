using Ambev.DeveloperEvaluation.Domain.Sales.Events;
using Ambev.DeveloperEvaluation.Sales.IntegrationEvents;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Ambev.DeveloperEvaluation.Application.Sales.Events;

public sealed class ItemCancelledDomainEventHandler(IBus bus, ILogger<ItemCancelledDomainEventHandler> logger)
    : INotificationHandler<ItemCancelledDomainEvent>
{
    public Task Handle(ItemCancelledDomainEvent notification, CancellationToken cancellationToken) =>
        SaleEventPublishing.PublishAsync(bus, logger, new ItemCancelledIntegrationEvent(
            notification.SaleId,
            notification.SaleNumber,
            notification.ItemId,
            notification.ProductId,
            notification.Title,
            notification.NewSaleTotal,
            notification.CancelledAt,
            notification.OccurredOnUtc));
}
