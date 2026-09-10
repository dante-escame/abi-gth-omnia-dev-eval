namespace Ambev.DeveloperEvaluation.Domain.Common;

public interface IDomainEvent
{
    Guid Id { get; }

    DateTime OccurredOnUtc { get; }
}
