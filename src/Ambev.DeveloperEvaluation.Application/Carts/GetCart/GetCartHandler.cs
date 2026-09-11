using Ambev.DeveloperEvaluation.Application.Carts.Common;
using Ambev.DeveloperEvaluation.Application.Common.Results;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Carts.GetCart;

public sealed class GetCartHandler(ICartRepository cartRepository)
    : IRequestHandler<GetCartQuery, Result<CartResult>>
{
    public async Task<Result<CartResult>> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        var cart = await cartRepository.GetByIdAsync(request.Id, cancellationToken);

        return cart is null || !CartLines.IsVisibleTo(cart, request.Caller)
            ? Result.Failure<CartResult>(CartErrors.NotFound(request.Id))
            : Result.Success(CartResult.From(cart));
    }
}
