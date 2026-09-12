using Ambev.DeveloperEvaluation.Application.Ports;

namespace Ambev.DeveloperEvaluation.Unit.Application.Carts.TestData;

public sealed class ReplicaDouble : IProductTitles
{
    private readonly Dictionary<Guid, Entry> _entries = [];

    public Task<IReadOnlyDictionary<Guid, string>> ResolveAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyDictionary<Guid, string> resolved = productIds
            .Where(id => _entries.TryGetValue(id, out var entry) && entry.Title is not null)
            .ToDictionary(id => id, id => _entries[id].Title!);

        return Task.FromResult(resolved);
    }

    public Task UpsertAsync(
        Guid productId,
        string title,
        DateTime occurredOnUtc,
        CancellationToken cancellationToken = default) =>
        WriteAsync(productId, title, occurredOnUtc);

    public Task RemoveAsync(Guid productId, DateTime occurredOnUtc, CancellationToken cancellationToken = default) =>
        WriteAsync(productId, null, occurredOnUtc);

    public async Task<string?> TitleOfAsync(Guid productId) =>
        (await ResolveAsync([productId])).GetValueOrDefault(productId);

    private Task WriteAsync(Guid productId, string? title, DateTime occurredOnUtc)
    {
        if (_entries.TryGetValue(productId, out var entry) && entry.UpdatedAtUtc > occurredOnUtc)
            return Task.CompletedTask;

        _entries[productId] = new Entry(title, occurredOnUtc);

        return Task.CompletedTask;
    }

    private sealed record Entry(string? Title, DateTime UpdatedAtUtc);
}
