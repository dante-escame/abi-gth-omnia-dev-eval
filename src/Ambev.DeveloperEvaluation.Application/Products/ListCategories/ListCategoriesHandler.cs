using Ambev.DeveloperEvaluation.Application.Ports;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Products.ListCategories;

public sealed class ListCategoriesHandler(IProductQueries productQueries)
    : IRequestHandler<ListCategoriesQuery, IReadOnlyList<string>>
{
    public Task<IReadOnlyList<string>> Handle(ListCategoriesQuery request, CancellationToken cancellationToken) =>
        productQueries.ListCategoriesAsync(cancellationToken);
}
