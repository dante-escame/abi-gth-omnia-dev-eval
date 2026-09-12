namespace Ambev.DeveloperEvaluation.Application.Ports;

public sealed record ProductSnapshot(Guid Id, string Title, decimal Price);

public interface IProductReader
{
    Task<ProductSnapshot?> GetAsync(Guid productId, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, ProductSnapshot>> GetManyAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken = default);
}
