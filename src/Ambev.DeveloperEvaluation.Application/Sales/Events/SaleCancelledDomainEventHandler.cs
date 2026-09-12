using Ambev.DeveloperEvaluation.Domain.Sales.Events;
using Ambev.DeveloperEvaluation.Sales.IntegrationEvents;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Ambev.DeveloperEvaluation.Application.Sales.Events;

public sealed class SaleCancelledDomainEventHandler(IBus bus, ILogger<SaleCancelledDomainEventHandler> logger)
    : INotificationHandler<SaleCancelledDomainEvent>
{
    public Task Handle(SaleCancelledDomainEvent notification, CancellationToken cancellationToken) =>
        SaleEventPublishing.PublishAsync(bus, logger, new SaleCancelledIntegrationEvent(
            notification.SaleId,
            notification.SaleNumber,
            notification.Reason,
            notification.CancelledAt,
            notification.OccurredOnUtc));
}
