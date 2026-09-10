namespace Ambev.DeveloperEvaluation.ORM.Outbox;

public sealed class OutboxOptions
{
    public int IntervalInSeconds { get; init; } = 5;

    public int BatchSize { get; init; } = 20;
}
