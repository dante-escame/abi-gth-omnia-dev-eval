using Ambev.DeveloperEvaluation.Application.Common.Lists;
using Ambev.DeveloperEvaluation.Application.Ports;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

public sealed class ListSalesHandler(ISaleQueries saleQueries, IPagedQueryExecutor executor)
    : IRequestHandler<ListSalesQuery, PagedResult<SaleResult>>
{
    public async Task<PagedResult<SaleResult>> Handle(ListSalesQuery request, CancellationToken cancellationToken)
    {
        var source = request.Caller.IsScoped
            ? saleQueries.QueryByCustomer(request.Caller.UserId)
            : saleQueries.Query();

        var query = source
            .ApplyFilters(request.List, SaleListFields.Map)
            .ApplyOrdering(request.List, SaleListFields.Map);

        var page = await executor.ToPagedResultAsync(query, request.List.Page, request.List.Size, cancellationToken);

        return page.Map(SaleResult.From);
    }
}
