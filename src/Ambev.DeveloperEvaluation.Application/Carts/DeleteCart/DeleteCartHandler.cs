using Ambev.DeveloperEvaluation.Application.Carts.Common;
using Ambev.DeveloperEvaluation.Application.Common.Results;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Carts.DeleteCart;

public sealed class DeleteCartHandler(ICartRepository cartRepository)
    : IRequestHandler<DeleteCartCommand, Result>
{
    public async Task<Result> Handle(DeleteCartCommand command, CancellationToken cancellationToken)
    {
        var cart = await cartRepository.GetByIdAsync(command.Id, cancellationToken);

        if (cart is null || !CartLines.IsVisibleTo(cart, command.Caller))
            return Result.Failure(CartErrors.NotFound(command.Id));

        if (cart.Status != CartStatus.Active)
            return Result.Failure(CartErrors.AlreadyCheckedOut(command.Id));

        bool deleted = await cartRepository.DeleteAsync(command.Id, cancellationToken);

        return deleted
            ? Result.Success()
            : Result.Failure(CartErrors.NotFound(command.Id));
    }
}
