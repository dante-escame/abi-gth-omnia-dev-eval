using Ambev.DeveloperEvaluation.Application.Common;
using Ambev.DeveloperEvaluation.Application.Common.Results;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

public sealed record CancelSaleItemCommand(Guid SaleId, Guid ItemId, CallerContext Caller)
    : IRequest<Result<SaleResult>>;
