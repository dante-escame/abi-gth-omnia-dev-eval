using Ambev.DeveloperEvaluation.Application.Common.Results;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Products.DeleteProduct;

public sealed record DeleteProductCommand(Guid Id) : IRequest<Result>;
