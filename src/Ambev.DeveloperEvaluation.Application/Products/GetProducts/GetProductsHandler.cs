using Ambev.DeveloperEvaluation.Application.Products.Common;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Products.GetProducts;

public sealed class GetProductsHandler(IProductRepository productRepository)
    : IRequestHandler<GetProductsQuery, IReadOnlyList<ProductResult>>
{
    public async Task<IReadOnlyList<ProductResult>> Handle(GetProductsQuery query, CancellationToken cancellationToken)
    {
        var products = await productRepository.GetByIdsAsync(query.Ids, cancellationToken);

        return products.Select(ProductResult.From).ToList();
    }
}
