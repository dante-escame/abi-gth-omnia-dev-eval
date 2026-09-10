using Ambev.DeveloperEvaluation.Persistence.Mongo.Products;
using MongoDB.Driver;

namespace Ambev.DeveloperEvaluation.Persistence.Mongo;

public sealed class CatalogContext(IMongoDatabase database)
{
    private const string ProductsCollection = "products";

    public IMongoCollection<ProductDocument> Products => database.GetCollection<ProductDocument>(ProductsCollection);
}
