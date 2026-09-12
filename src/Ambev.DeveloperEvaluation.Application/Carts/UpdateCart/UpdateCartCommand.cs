using Ambev.DeveloperEvaluation.Application.Carts.Common;
using Ambev.DeveloperEvaluation.Application.Common;
using Ambev.DeveloperEvaluation.Application.Common.Results;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Carts.UpdateCart;

public sealed record UpdateCartCommand(Guid Id, CallerContext Caller, IReadOnlyList<CartLineInput> Products)
    : IRequest<Result<CartResult>>;
