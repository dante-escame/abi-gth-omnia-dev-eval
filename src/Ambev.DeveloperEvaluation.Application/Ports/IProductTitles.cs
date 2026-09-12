namespace Ambev.DeveloperEvaluation.Application.Ports;

public interface IProductTitles
{
    Task<IReadOnlyDictionary<Guid, string>> ResolveAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken = default);

    Task UpsertAsync(Guid productId, string title, DateTime occurredOnUtc, CancellationToken cancellationToken = default);

    Task RemoveAsync(Guid productId, DateTime occurredOnUtc, CancellationToken cancellationToken = default);
}
