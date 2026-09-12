using Ambev.DeveloperEvaluation.Domain.Carts;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using MongoDB.Driver;

namespace Ambev.DeveloperEvaluation.Persistence.Mongo.Carts;

public sealed class MongoCartRepository(CartContext context) : ICartRepository
{
    public async Task<Cart> CreateAsync(Cart cart, CancellationToken cancellationToken = default)
    {
        await context.Carts.InsertOneAsync(CartDocument.From(cart), cancellationToken: cancellationToken);
        return cart;
    }

    public async Task<Cart?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var document = await context.Carts
            .Find(cart => cart.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        return document?.ToAggregate();
    }

    public async Task<Cart> UpdateAsync(Cart cart, CancellationToken cancellationToken = default)
    {
        await context.Carts.ReplaceOneAsync(
            document => document.Id == cart.Id,
            CartDocument.From(cart),
            cancellationToken: cancellationToken);

        return cart;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await context.Carts.DeleteOneAsync(cart => cart.Id == id, cancellationToken);
        return result.DeletedCount > 0;
    }
}
