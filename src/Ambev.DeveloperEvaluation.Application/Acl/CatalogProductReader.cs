using Ambev.DeveloperEvaluation.Application.Ports;
using Ambev.DeveloperEvaluation.Application.Products.Common;
using Ambev.DeveloperEvaluation.Application.Products.GetProducts;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Acl;

public sealed class CatalogProductReader(ISender sender) : IProductReader
{
    public async Task<ProductSnapshot?> GetAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var products = await sender.Send(new GetProductsQuery([productId]), cancellationToken);
        return products.Count == 0 ? null : Translate(products[0]);
    }

    public async Task<IReadOnlyDictionary<Guid, ProductSnapshot>> GetManyAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken = default)
    {
        if (productIds.Count == 0)
            return new Dictionary<Guid, ProductSnapshot>();

        var products = await sender.Send(new GetProductsQuery(productIds), cancellationToken);
        return products.ToDictionary(product => product.Id, Translate);
    }

    private static ProductSnapshot Translate(ProductResult product) =>
        new(product.Id, product.Title, product.Price);
}
