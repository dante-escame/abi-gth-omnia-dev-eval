using Ambev.DeveloperEvaluation.Application.Common.Lists;
using Ambev.DeveloperEvaluation.Application.Ports;
using Ambev.DeveloperEvaluation.Application.Products.Common;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Products.ListProducts;

public sealed class ListProductsHandler(IProductQueries productQueries, IDocumentPagedQueryExecutor executor)
    : IRequestHandler<ListProductsQuery, PagedResult<ProductResult>>
{
    private const string CategoryField = "category";

    public async Task<PagedResult<ProductResult>> Handle(ListProductsQuery request, CancellationToken cancellationToken)
    {
        var category = request.List.Filters.FirstOrDefault(filter =>
            filter.Operator == FilterOperator.Equal &&
            string.Equals(filter.Field, CategoryField, StringComparison.OrdinalIgnoreCase));

        var list = category is null
            ? request.List
            : request.List with { Filters = request.List.Filters.Where(filter => filter != category).ToList() };

        var source = category is null
            ? productQueries.Query()
            : productQueries.QueryByCategory(category.Value);

        var query = source
            .ApplyFilters(list, ProductListFields.Map)
            .ApplyOrdering(list, ProductListFields.Map);

        var page = await executor.ToPagedResultAsync(query, list.Page, list.Size, cancellationToken);
        return page.Map(ProductResult.From);
    }
}
