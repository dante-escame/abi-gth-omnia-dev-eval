using Ambev.DeveloperEvaluation.Persistence.Mongo.Products;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Rebus.Bus;

namespace Ambev.DeveloperEvaluation.Persistence.Mongo.Outbox;

public sealed class ProcessCatalogOutboxJob(
    CatalogContext context,
    IBus bus,
    IOptions<CatalogOutboxOptions> outboxOptions,
    ILogger<ProcessCatalogOutboxJob> logger)
    : BackgroundService
{
    private static readonly FilterDefinition<ProductDocument> HasPendingEvents =
        Builders<ProductDocument>.Filter.Exists("pendingEvents.0");

    private readonly CatalogOutboxOptions _options = outboxOptions.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_options.IntervalInSeconds));

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await ProcessBatchAsync(stoppingToken);
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "Catalog outbox cycle failed");
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    public async Task<int> ProcessBatchAsync(CancellationToken cancellationToken = default)
    {
        var documents = await context.Products
            .Find(HasPendingEvents)
            .Limit(_options.BatchSize)
            .ToListAsync(cancellationToken);

        var published = 0;

        foreach (var document in documents)
            published += await PublishAsync(document, cancellationToken);

        return published;
    }

    private async Task<int> PublishAsync(ProductDocument document, CancellationToken cancellationToken)
    {
        var sent = new List<Guid>();

        foreach (var pending in document.PendingEvents.OrderBy(entry => entry.OccurredOnUtc))
        {
            try
            {
                var integrationEvent = CatalogIntegrationEventFactory.From(pending);

                if (integrationEvent is not null)
                    await bus.Publish(integrationEvent);

                sent.Add(pending.Id);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Catalog outbox entry {PendingEventId} failed", pending.Id);
            }
        }

        if (sent.Count == 0)
            return 0;

        if (document.DeletedAt is not null && sent.Count == document.PendingEvents.Count)
        {
            await context.Products.DeleteOneAsync(
                candidate => candidate.Id == document.Id,
                cancellationToken);

            return sent.Count;
        }

        await context.Products.UpdateOneAsync(
            candidate => candidate.Id == document.Id,
            Builders<ProductDocument>.Update.PullFilter(
                candidate => candidate.PendingEvents,
                Builders<PendingEventDocument>.Filter.In(entry => entry.Id, sent)),
            cancellationToken: cancellationToken);

        return sent.Count;
    }
}
