using Ambev.DeveloperEvaluation.Application.Common.Lists;
using Ambev.DeveloperEvaluation.Application.Ports;
using Ambev.DeveloperEvaluation.Application.Products.Common;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Products.ListProductsByCategory;

public sealed class ListProductsByCategoryHandler(
    IProductQueries productQueries,
    IDocumentPagedQueryExecutor executor)
    : IRequestHandler<ListProductsByCategoryQuery, PagedResult<ProductResult>>
{
    public async Task<PagedResult<ProductResult>> Handle(
        ListProductsByCategoryQuery request,
        CancellationToken cancellationToken)
    {
        var query = productQueries.QueryByCategory(request.Category)
            .ApplyFilters(request.List, ProductListFields.Map)
            .ApplyOrdering(request.List, ProductListFields.Map);

        var page = await executor.ToPagedResultAsync(query, request.List.Page, request.List.Size, cancellationToken);
        return page.Map(ProductResult.From);
    }
}
