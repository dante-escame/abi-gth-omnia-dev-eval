using Ambev.DeveloperEvaluation.Application.Common.Lists;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Lists;

public sealed class PagedQueryExecutor : IPagedQueryExecutor
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
