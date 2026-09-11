using Ambev.DeveloperEvaluation.Application.Carts.Common;
using Ambev.DeveloperEvaluation.Application.Common;
using Ambev.DeveloperEvaluation.Application.Common.Results;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Carts.CreateCart;

public sealed record CreateCartCommand(CallerContext Caller, IReadOnlyList<CartLineInput> Products)
    : IRequest<Result<CartResult>>;
