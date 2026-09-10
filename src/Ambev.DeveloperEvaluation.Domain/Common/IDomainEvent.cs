using MediatR;

namespace Ambev.DeveloperEvaluation.Domain.Common;

public interface IDomainEvent : INotification
{
    Guid Id { get; }

    DateTime OccurredOnUtc { get; }
}
