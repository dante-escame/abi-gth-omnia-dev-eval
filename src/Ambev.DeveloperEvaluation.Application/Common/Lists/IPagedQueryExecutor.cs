namespace Ambev.DeveloperEvaluation.Application.Common.Lists;

public interface IPagedQueryExecutor
{
    Task<PagedResult<T>> ToPagedResultAsync<T>(
        IQueryable<T> source, 
        int page, 
        int size, 
        CancellationToken cancellationToken = default);
}
