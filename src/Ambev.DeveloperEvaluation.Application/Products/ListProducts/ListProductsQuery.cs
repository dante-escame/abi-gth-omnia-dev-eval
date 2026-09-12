using Ambev.DeveloperEvaluation.Application.Common.Lists;
using Ambev.DeveloperEvaluation.Application.Products.Common;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Products.ListProducts;

public sealed record ListProductsQuery(ListQuery List) : IRequest<PagedResult<ProductResult>>;
