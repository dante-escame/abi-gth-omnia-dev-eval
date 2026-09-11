using Ambev.DeveloperEvaluation.Application.Carts.Common;
using Ambev.DeveloperEvaluation.Application.Common;
using Ambev.DeveloperEvaluation.Application.Common.Results;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Carts.GetCart;

public sealed record GetCartQuery(Guid Id, CallerContext Caller) : IRequest<Result<CartResult>>;
