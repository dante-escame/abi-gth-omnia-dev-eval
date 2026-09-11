using Ambev.DeveloperEvaluation.Application.Products.Common;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Products.GetProducts;

public sealed record GetProductsQuery(IReadOnlyCollection<Guid> Ids) : IRequest<IReadOnlyList<ProductResult>>;
