using Ambev.DeveloperEvaluation.Application.Common.Results;
using Ambev.DeveloperEvaluation.Application.Products.Common;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Products.CreateProduct;

public class CreateProductCommand : IRequest<Result<ProductResult>>
{
    public string Title { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string Description { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string Image { get; set; } = string.Empty;

    public RatingInput Rating { get; set; } = RatingInput.Empty;
}
