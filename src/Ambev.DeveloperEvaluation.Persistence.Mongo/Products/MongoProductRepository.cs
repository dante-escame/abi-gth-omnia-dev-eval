using Ambev.DeveloperEvaluation.Domain.Products;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Persistence.Mongo.Outbox;
using MongoDB.Driver;

namespace Ambev.DeveloperEvaluation.Persistence.Mongo.Products;

public sealed class MongoProductRepository(CatalogContext context) : IProductRepository
{
    public async Task<Product> CreateAsync(Product product, CancellationToken cancellationToken = default)
    {
        await context.Products.InsertOneAsync(ProductDocument.From(product), cancellationToken: cancellationToken);
        product.ClearDomainEvents();

        return product;
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var document = await context.Products
            .Find(product => product.Id == id && product.DeletedAt == null)
            .FirstOrDefaultAsync(cancellationToken);

        return document?.ToAggregate();
    }

    public async Task<IReadOnlyList<Product>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
            return [];

        var documents = await context.Products
            .Find(product => ids.Contains(product.Id) && product.DeletedAt == null)
            .ToListAsync(cancellationToken);

        return documents.Select(document => document.ToAggregate()).ToList();
    }

    public async Task<Product> UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        var update = Builders<ProductDocument>.Update
            .Set(document => document.Title, product.Title.Value)
            .Set(document => document.Price, product.Price.Amount)
            .Set(document => document.Description, product.Description)
            .Set(document => document.Category, product.Category.Name)
            .Set(document => document.Image, product.Image.Value)
            .Set(document => document.Rate, product.Rating.Rate)
            .Set(document => document.RatingCount, product.Rating.Count)
            .Set(document => document.UpdatedAt, product.UpdatedAt);

        await ApplyAsync(product, update, cancellationToken);

        return product;
    }

    public async Task<bool> DeleteAsync(Product product, CancellationToken cancellationToken = default)
    {
        var update = Builders<ProductDocument>.Update.Set(document => document.DeletedAt, DateTime.UtcNow);

        var result = await ApplyAsync(product, update, cancellationToken);

        return result.MatchedCount > 0;
    }

    private async Task<UpdateResult> ApplyAsync(
        Product product,
        UpdateDefinition<ProductDocument> update,
        CancellationToken cancellationToken)
    {
        var pending = product.DomainEvents.Select(DocumentOutboxSerializer.ToPending).ToList();

        if (pending.Count > 0)
            update = update.PushEach(document => document.PendingEvents, pending);

        var result = await context.Products.UpdateOneAsync(
            document => document.Id == product.Id && document.DeletedAt == null,
            update,
            cancellationToken: cancellationToken);

        product.ClearDomainEvents();

        return result;
    }
}
