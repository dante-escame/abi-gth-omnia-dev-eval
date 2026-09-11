using Ambev.DeveloperEvaluation.Application.Ports;
using Ambev.DeveloperEvaluation.Domain.Products.Events;
using Ambev.DeveloperEvaluation.Persistence.Mongo.Outbox;
using MongoDB.Driver;

namespace Ambev.DeveloperEvaluation.Persistence.Mongo.Products;

public sealed class MongoProductEventReplay(CatalogContext context) : IProductEventReplay
{
    public async Task<long> EnqueueSnapshotsAsync(CancellationToken cancellationToken = default)
    {
        var documents = await context.Products
            .Find(document => document.DeletedAt == null)
            .Project(document => new { document.Id, document.Title })
            .ToListAsync(cancellationToken);

        if (documents.Count == 0)
            return 0;

        var writes = documents
            .Select(document => new UpdateOneModel<ProductDocument>(
                Builders<ProductDocument>.Filter.Eq(candidate => candidate.Id, document.Id),
                Builders<ProductDocument>.Update.Push(
                    candidate => candidate.PendingEvents,
                    DocumentOutboxSerializer.ToPending(new ProductUpdatedDomainEvent(document.Id, document.Title)))))
            .ToList<WriteModel<ProductDocument>>();

        var result = await context.Products.BulkWriteAsync(writes, cancellationToken: cancellationToken);

        return result.ModifiedCount;
    }
}
