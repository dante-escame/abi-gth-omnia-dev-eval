using Ambev.DeveloperEvaluation.Application.Common.Results;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Products.DeleteProduct;

public sealed class DeleteProductHandler(IProductRepository productRepository)
    : IRequestHandler<DeleteProductCommand, Result>
{
    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null)
            return Result.Failure(ProductErrors.NotFound(request.Id));

        product.Delete();

        bool deleted = await productRepository.DeleteAsync(product, cancellationToken);

        return deleted
            ? Result.Success()
            : Result.Failure(ProductErrors.NotFound(request.Id));
    }
}
