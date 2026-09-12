using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Products.ListCategories;

public sealed record ListCategoriesQuery : IRequest<IReadOnlyList<string>>;
