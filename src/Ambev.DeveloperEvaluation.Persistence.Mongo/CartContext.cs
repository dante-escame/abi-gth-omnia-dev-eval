using Ambev.DeveloperEvaluation.Persistence.Mongo.Carts;
using MongoDB.Driver;

namespace Ambev.DeveloperEvaluation.Persistence.Mongo;

public sealed class CartContext(IMongoDatabase database)
{
    private const string CartsCollection = "carts";

    private const string ProductTitlesCollection = "product_titles";

    public IMongoCollection<CartDocument> Carts => database.GetCollection<CartDocument>(CartsCollection);

    public IMongoCollection<ProductTitleDocument> ProductTitles =>
        database.GetCollection<ProductTitleDocument>(ProductTitlesCollection);
}
