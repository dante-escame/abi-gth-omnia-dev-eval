using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Carts.CreateCart;

public class CreateCartCommandValidator : AbstractValidator<CreateCartCommand>
{
    public CreateCartCommandValidator()
    {
        RuleFor(cart => cart.Products).NotEmpty();
        RuleForEach(cart => cart.Products).ChildRules(line =>
        {
            line.RuleFor(product => product.ProductId).NotEmpty();
            line.RuleFor(product => product.Quantity).GreaterThanOrEqualTo(1);
        });
    }
}
