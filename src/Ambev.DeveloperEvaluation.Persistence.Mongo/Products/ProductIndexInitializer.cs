using Microsoft.Extensions.Hosting;
using MongoDB.Driver;

namespace Ambev.DeveloperEvaluation.Persistence.Mongo.Products;

public sealed class ProductIndexInitializer(CatalogContext context) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        var keys = Builders<ProductDocument>.IndexKeys;

        var models = new[]
        {
            new CreateIndexModel<ProductDocument>(keys.Ascending(product => product.Category)),
            new CreateIndexModel<ProductDocument>(keys.Ascending(product => product.Price)),
            new CreateIndexModel<ProductDocument>(keys.Ascending(product => product.Title))
        };

        return context.Products.Indexes.CreateManyAsync(models, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
