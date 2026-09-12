namespace Ambev.DeveloperEvaluation.Persistence.Mongo.Outbox;

public sealed class CatalogOutboxOptions
{
    public int IntervalInSeconds { get; init; } = 5;

    public int BatchSize { get; init; } = 20;
}
