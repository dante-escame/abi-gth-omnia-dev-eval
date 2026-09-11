using Ambev.DeveloperEvaluation.Domain.Sales.Events;
using Ambev.DeveloperEvaluation.Sales.IntegrationEvents;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Ambev.DeveloperEvaluation.Application.Sales.Events;

public sealed class SaleCreatedDomainEventHandler(IBus bus, ILogger<SaleCreatedDomainEventHandler> logger)
    : INotificationHandler<SaleCreatedDomainEvent>
{
    public Task Handle(SaleCreatedDomainEvent notification, CancellationToken cancellationToken) =>
        SaleEventPublishing.PublishAsync(bus, logger, new SaleCreatedIntegrationEvent(
            notification.SaleId,
            notification.SaleNumber,
            notification.CartId,
            notification.CustomerId,
            notification.CustomerName,
            notification.BranchId,
            notification.BranchName,
            notification.Items.Select(SoldItems.From).ToList(),
            notification.Total,
            notification.CreatedAt,
            notification.OccurredOnUtc));
}
