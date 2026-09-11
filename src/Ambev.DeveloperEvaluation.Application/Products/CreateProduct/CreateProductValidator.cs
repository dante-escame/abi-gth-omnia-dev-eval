using Ambev.DeveloperEvaluation.Domain.Products.ValueObjects;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Products.CreateProduct;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(product => product.Title).NotEmpty().MaximumLength(200);
        RuleFor(product => product.Price).GreaterThan(0);
        RuleFor(product => product.Category).NotEmpty().MaximumLength(100);
        RuleFor(product => product.Image).NotEmpty().Must(ImageUrl.IsValid);
        RuleFor(product => product.Rating.Rate).InclusiveBetween(0m, 5m);
        RuleFor(product => product.Rating.Count).GreaterThanOrEqualTo(0);
    }
}
