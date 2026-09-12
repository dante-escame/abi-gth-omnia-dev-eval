namespace Ambev.DeveloperEvaluation.Catalog.IntegrationEvents;

public sealed record ProductCreatedIntegrationEvent(Guid ProductId, string Title, DateTime OccurredOnUtc);

public sealed record ProductUpdatedIntegrationEvent(Guid ProductId, string Title, DateTime OccurredOnUtc);

public sealed record ProductDeletedIntegrationEvent(Guid ProductId, DateTime OccurredOnUtc);
