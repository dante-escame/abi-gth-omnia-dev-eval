using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Carts.Replication;

public sealed record RemoveProductTitleCommand(Guid ProductId, DateTime OccurredOnUtc) : IRequest;
