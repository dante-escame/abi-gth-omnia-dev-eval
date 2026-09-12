using Ambev.DeveloperEvaluation.Application.Ports;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Ambev.DeveloperEvaluation.Persistence.Mongo.Carts;

public sealed class MongoProductTitles(CartContext context, ILogger<MongoProductTitles> logger) : IProductTitles
{
    public async Task<IReadOnlyDictionary<Guid, string>> ResolveAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken = default)
    {
        if (productIds.Count == 0)
            return new Dictionary<Guid, string>();

        var documents = await context.ProductTitles
            .Find(Builders<ProductTitleDocument>.Filter.In(document => document.ProductId, productIds))
            .ToListAsync(cancellationToken);

        return documents
            .Where(document => document.Title is not null)
            .ToDictionary(document => document.ProductId, document => document.Title!);
    }

    public Task UpsertAsync(
        Guid productId,
        string title,
        DateTime occurredOnUtc,
        CancellationToken cancellationToken = default) =>
        WriteAsync(productId, title, occurredOnUtc, cancellationToken);

    public Task RemoveAsync(Guid productId, DateTime occurredOnUtc, CancellationToken cancellationToken = default) =>
        WriteAsync(productId, null, occurredOnUtc, cancellationToken);

    private async Task WriteAsync(
        Guid productId,
        string? title,
        DateTime occurredOnUtc,
        CancellationToken cancellationToken)
    {
        var filter = Builders<ProductTitleDocument>.Filter.And(
            Builders<ProductTitleDocument>.Filter.Eq(document => document.ProductId, productId),
            Builders<ProductTitleDocument>.Filter.Lte(document => document.UpdatedAtUtc, occurredOnUtc));

        var update = Builders<ProductTitleDocument>.Update
            .Set(document => document.Title, title)
            .Set(document => document.UpdatedAtUtc, occurredOnUtc);

        try
        {
            await context.ProductTitles.UpdateOneAsync(
                filter,
                update,
                new UpdateOptions { IsUpsert = true },
                cancellationToken);
        }
        catch (MongoWriteException exception) when (exception.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            logger.LogDebug(
                "Product title {ProductId} already holds a newer snapshot than {OccurredOnUtc}",
                productId,
                occurredOnUtc);
        }
    }
}
