namespace Ambev.DeveloperEvaluation.Application.Ports;

public interface IOutboxConsumerTracker
{
    Task<bool> HasRunAsync(Guid outboxMessageId, string handlerName, CancellationToken cancellationToken = default);

    Task MarkAsRunAsync(Guid outboxMessageId, string handlerName, CancellationToken cancellationToken = default);
}
