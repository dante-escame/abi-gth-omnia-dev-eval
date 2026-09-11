using Microsoft.Extensions.Hosting;
using MongoDB.Driver;

namespace Ambev.DeveloperEvaluation.Persistence.Mongo.Carts;

public sealed class CartIndexInitializer(CartContext context) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        var keys = Builders<CartDocument>.IndexKeys;

        var models = new[]
        {
            new CreateIndexModel<CartDocument>(keys.Ascending(cart => cart.UserId)),
            new CreateIndexModel<CartDocument>(keys.Ascending(cart => cart.CreatedAt)),
            new CreateIndexModel<CartDocument>(keys.Ascending(cart => cart.Status))
        };

        return context.Carts.Indexes.CreateManyAsync(models, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
