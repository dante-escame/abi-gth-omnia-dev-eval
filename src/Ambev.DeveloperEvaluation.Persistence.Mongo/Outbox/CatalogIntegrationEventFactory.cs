using Ambev.DeveloperEvaluation.Catalog.IntegrationEvents;
using Ambev.DeveloperEvaluation.Domain.Products.Events;

namespace Ambev.DeveloperEvaluation.Persistence.Mongo.Outbox;

public static class CatalogIntegrationEventFactory
{
    public static object? From(PendingEventDocument pending) => DocumentOutboxSerializer.ToDomainEvent(pending) switch
    {
        ProductCreatedDomainEvent created =>
            new ProductCreatedIntegrationEvent(created.ProductId, created.Title, created.OccurredOnUtc),
        ProductUpdatedDomainEvent updated =>
            new ProductUpdatedIntegrationEvent(updated.ProductId, updated.Title, updated.OccurredOnUtc),
        ProductDeletedDomainEvent deleted =>
            new ProductDeletedIntegrationEvent(deleted.ProductId, deleted.OccurredOnUtc),
        _ => null
    };
}
