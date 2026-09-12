using Ambev.DeveloperEvaluation.Application.Common;
using Ambev.DeveloperEvaluation.Application.Common.Results;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;

public sealed record DeleteSaleCommand(Guid Id, CallerContext Caller) : IRequest<Result>;
