using Ambev.DeveloperEvaluation.Application.Common.Results;
using Ambev.DeveloperEvaluation.Application.Products.Common;
using Ambev.DeveloperEvaluation.Domain.Products.ValueObjects;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Products.UpdateProduct;

public sealed class UpdateProductHandler(IProductRepository productRepository)
    : IRequestHandler<UpdateProductCommand, Result<ProductResult>>
{
    public async Task<Result<ProductResult>> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(command.Id, cancellationToken);
        if (product is null)
            return Result.Failure<ProductResult>(ProductErrors.NotFound(command.Id));

        product.UpdateDetails(
            new ProductTitle(command.Title),
            command.Description,
            new Category(command.Category),
            new ImageUrl(command.Image));

        product.Reprice(new Money(command.Price));
        product.SetRating(new Rating(command.Rating.Rate, command.Rating.Count));

        await productRepository.UpdateAsync(product, cancellationToken);

        return Result.Success(ProductResult.From(product));
    }
}
