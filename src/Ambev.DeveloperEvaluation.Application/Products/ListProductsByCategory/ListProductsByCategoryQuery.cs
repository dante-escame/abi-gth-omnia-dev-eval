using Ambev.DeveloperEvaluation.Application.Common.Lists;
using Ambev.DeveloperEvaluation.Application.Products.Common;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Products.ListProductsByCategory;

public sealed record ListProductsByCategoryQuery(string Category, ListQuery List)
    : IRequest<PagedResult<ProductResult>>;
