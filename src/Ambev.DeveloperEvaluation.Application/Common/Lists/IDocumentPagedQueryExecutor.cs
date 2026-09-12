namespace Ambev.DeveloperEvaluation.Application.Common.Lists;

public interface IDocumentPagedQueryExecutor
{
    Task<PagedResult<T>> ToPagedResultAsync<T>(
        IQueryable<T> source,
        int page,
        int size,
        CancellationToken cancellationToken = default);
}
