using Ambev.DeveloperEvaluation.Domain.Products;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using MongoDB.Driver;

namespace Ambev.DeveloperEvaluation.Persistence.Mongo.Products;

public sealed class MongoProductRepository(CatalogContext context) : IProductRepository
{
    public async Task<Product> CreateAsync(Product product, CancellationToken cancellationToken = default)
    {
        await context.Products.InsertOneAsync(ProductDocument.From(product), cancellationToken: cancellationToken);
        return product;
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var document = await context.Products
            .Find(product => product.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        return document?.ToAggregate();
    }

    public async Task<Product> UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        await context.Products.ReplaceOneAsync(
            document => document.Id == product.Id,
            ProductDocument.From(product),
            cancellationToken: cancellationToken);

        return product;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await context.Products.DeleteOneAsync(product => product.Id == id, cancellationToken);
        return result.DeletedCount > 0;
    }
}
