using Ambev.DeveloperEvaluation.Application.Carts.Common;
using Ambev.DeveloperEvaluation.Application.Common.Results;
using Ambev.DeveloperEvaluation.Application.Ports;
using Ambev.DeveloperEvaluation.Domain.Carts;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Carts.CreateCart;

public sealed class CreateCartHandler(ICartRepository cartRepository, IProductReader productReader)
    : IRequestHandler<CreateCartCommand, Result<CartResult>>
{
    public async Task<Result<CartResult>> Handle(CreateCartCommand command, CancellationToken cancellationToken)
    {
        var items = await CartLines.BuildAsync(command.Products, productReader, cancellationToken);

        var cart = Cart.Create(command.Caller.UserId, items);

        await cartRepository.CreateAsync(cart, cancellationToken);

        return Result.Success(CartResult.From(cart));
    }
}
