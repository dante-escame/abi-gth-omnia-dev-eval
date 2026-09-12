using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Carts.Replication;

public sealed record UpsertProductTitleCommand(Guid ProductId, string Title, DateTime OccurredOnUtc) : IRequest;
