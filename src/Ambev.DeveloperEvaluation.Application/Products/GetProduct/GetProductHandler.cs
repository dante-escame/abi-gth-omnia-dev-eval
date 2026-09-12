using Ambev.DeveloperEvaluation.Application.Common.Results;
using Ambev.DeveloperEvaluation.Application.Products.Common;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Products.GetProduct;

public sealed class GetProductHandler(IProductRepository productRepository)
    : IRequestHandler<GetProductQuery, Result<ProductResult>>
{
    public async Task<Result<ProductResult>> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.Id, cancellationToken);

        return product is null
            ? Result.Failure<ProductResult>(ProductErrors.NotFound(request.Id))
            : Result.Success(ProductResult.From(product));
    }
}
