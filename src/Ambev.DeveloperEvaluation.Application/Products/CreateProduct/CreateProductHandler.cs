using Ambev.DeveloperEvaluation.Application.Common.Results;
using Ambev.DeveloperEvaluation.Application.Products.Common;
using Ambev.DeveloperEvaluation.Domain.Products;
using Ambev.DeveloperEvaluation.Domain.Products.ValueObjects;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Products.CreateProduct;

public sealed class CreateProductHandler(IProductRepository productRepository)
    : IRequestHandler<CreateProductCommand, Result<ProductResult>>
{
    public async Task<Result<ProductResult>> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var product = Product.Create(
            new ProductTitle(command.Title),
            new Money(command.Price),
            command.Description,
            new Category(command.Category),
            new ImageUrl(command.Image),
            new Rating(command.Rating.Rate, command.Rating.Count));

        await productRepository.CreateAsync(product, cancellationToken);

        return Result.Success(ProductResult.From(product));
    }
}
