using Ambev.DeveloperEvaluation.Application.Common;
using Ambev.DeveloperEvaluation.Application.Common.Results;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSale;

public sealed record CancelSaleCommand(Guid Id, CallerContext Caller, string? Reason)
    : IRequest<Result<SaleResult>>;
