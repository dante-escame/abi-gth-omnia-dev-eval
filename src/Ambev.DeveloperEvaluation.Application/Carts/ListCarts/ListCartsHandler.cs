using Ambev.DeveloperEvaluation.Application.Carts.Common;
using Ambev.DeveloperEvaluation.Application.Common.Lists;
using Ambev.DeveloperEvaluation.Application.Ports;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Carts.ListCarts;

public sealed class ListCartsHandler(ICartQueries cartQueries, IDocumentPagedQueryExecutor executor)
    : IRequestHandler<ListCartsQuery, PagedResult<CartResult>>
{
    public async Task<PagedResult<CartResult>> Handle(ListCartsQuery request, CancellationToken cancellationToken)
    {
        var source = request.Caller.IsScoped
            ? cartQueries.QueryByCustomer(request.Caller.UserId)
            : cartQueries.Query();

        var query = source
            .ApplyFilters(request.List, CartListFields.Map)
            .ApplyOrdering(request.List, CartListFields.Map);

        var page = await executor.ToPagedResultAsync(query, request.List.Page, request.List.Size, cancellationToken);

        return page.Map(CartResult.From);
    }
}
