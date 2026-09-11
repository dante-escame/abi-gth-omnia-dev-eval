using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Products.UpdateProduct;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(product => product.Id).NotEmpty();
        RuleFor(product => product.Title).NotEmpty().MaximumLength(200);
        RuleFor(product => product.Price).GreaterThan(0);
        RuleFor(product => product.Category).NotEmpty().MaximumLength(100);
        RuleFor(product => product.Image).NotEmpty().Must(BeAnAbsoluteUrl);
        RuleFor(product => product.Rating.Rate).InclusiveBetween(0m, 5m);
        RuleFor(product => product.Rating.Count).GreaterThanOrEqualTo(0);
    }

    private static bool BeAnAbsoluteUrl(string value) =>
        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        Uri.TryCreate(value?.Trim() ?? string.Empty, UriKind.Absolute, out _);
}
