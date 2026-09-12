using Ambev.DeveloperEvaluation.Application.Products.Common;

namespace Ambev.DeveloperEvaluation.Application.Ports;

public interface IProductQueries
{
    IQueryable<ProductListItem> Query();

    IQueryable<ProductListItem> QueryByCategory(string category);

    Task<IReadOnlyList<string>> ListCategoriesAsync(CancellationToken cancellationToken = default);
}
