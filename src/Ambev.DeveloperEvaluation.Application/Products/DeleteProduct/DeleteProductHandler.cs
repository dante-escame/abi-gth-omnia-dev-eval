using Ambev.DeveloperEvaluation.Application.Common.Results;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Products.DeleteProduct;

public sealed class DeleteProductHandler(IProductRepository productRepository)
    : IRequestHandler<DeleteProductCommand, Result>
{
    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        bool deleted = await productRepository.DeleteAsync(request.Id, cancellationToken);

        return deleted
            ? Result.Success()
            : Result.Failure(ProductErrors.NotFound(request.Id));
    }
}
