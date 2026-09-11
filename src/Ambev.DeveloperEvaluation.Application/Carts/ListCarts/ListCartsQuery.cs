using Ambev.DeveloperEvaluation.Application.Carts.Common;
using Ambev.DeveloperEvaluation.Application.Common;
using Ambev.DeveloperEvaluation.Application.Common.Lists;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Carts.ListCarts;

public sealed record ListCartsQuery(ListQuery List, CallerContext Caller) : IRequest<PagedResult<CartResult>>;
