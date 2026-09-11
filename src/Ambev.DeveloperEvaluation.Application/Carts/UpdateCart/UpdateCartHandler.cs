using Ambev.DeveloperEvaluation.Application.Carts.Common;
using Ambev.DeveloperEvaluation.Application.Common.Results;
using Ambev.DeveloperEvaluation.Application.Ports;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Carts.UpdateCart;

public sealed class UpdateCartHandler(ICartRepository cartRepository, IProductReader productReader)
    : IRequestHandler<UpdateCartCommand, Result<CartResult>>
{
    public async Task<Result<CartResult>> Handle(UpdateCartCommand command, CancellationToken cancellationToken)
    {
        var cart = await cartRepository.GetByIdAsync(command.Id, cancellationToken);

        if (cart is null || !CartLines.IsVisibleTo(cart, command.Caller))
            return Result.Failure<CartResult>(CartErrors.NotFound(command.Id));

        if (cart.Status != CartStatus.Active)
            return Result.Failure<CartResult>(CartErrors.AlreadyCheckedOut(command.Id));

        var items = await CartLines.BuildAsync(command.Products, productReader, cancellationToken);

        cart.ReplaceItems(items);

        await cartRepository.UpdateAsync(cart, cancellationToken);

        return Result.Success(CartResult.From(cart));
    }
}
