using Ambev.DeveloperEvaluation.Application.Common;
using Ambev.DeveloperEvaluation.Application.Common.Results;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Carts.DeleteCart;

public sealed record DeleteCartCommand(Guid Id, CallerContext Caller) : IRequest<Result>;
