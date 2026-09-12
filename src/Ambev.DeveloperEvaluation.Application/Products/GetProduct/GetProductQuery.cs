using Ambev.DeveloperEvaluation.Application.Common.Results;
using Ambev.DeveloperEvaluation.Application.Products.Common;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Products.GetProduct;

public sealed record GetProductQuery(Guid Id) : IRequest<Result<ProductResult>>;
