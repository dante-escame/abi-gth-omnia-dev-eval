using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Carts.UpdateCart;

public class UpdateCartCommandValidator : AbstractValidator<UpdateCartCommand>
{
    public UpdateCartCommandValidator()
    {
        RuleFor(cart => cart.Id).NotEmpty();
        RuleFor(cart => cart.Products).NotEmpty();
        RuleForEach(cart => cart.Products).ChildRules(line =>
        {
            line.RuleFor(product => product.ProductId).NotEmpty();
            line.RuleFor(product => product.Quantity).GreaterThanOrEqualTo(1);
        });
    }
}
