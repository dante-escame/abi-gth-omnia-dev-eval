using Ambev.DeveloperEvaluation.Application.Common;
using Ambev.DeveloperEvaluation.Application.Common.Lists;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

public sealed record ListSalesQuery(ListQuery List, CallerContext Caller) : IRequest<PagedResult<SaleResult>>;
