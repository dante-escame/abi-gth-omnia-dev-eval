using Ambev.DeveloperEvaluation.Application.Common.Lists;
using MongoDB.Driver.Linq;

namespace Ambev.DeveloperEvaluation.Persistence.Mongo.Lists;

public sealed class DocumentPagedQueryExecutor : IDocumentPagedQueryExecutor
{
    public async Task<PagedResult<T>> ToPagedResultAsync<T>(
        IQueryable<T> source,
        int page,
        int size,
        CancellationToken cancellationToken = default)
    {
        int totalItems = await source.CountAsync(cancellationToken);

        var data = await source
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken);

        return PagedResult<T>.From(data, totalItems, page, size);
    }
}
