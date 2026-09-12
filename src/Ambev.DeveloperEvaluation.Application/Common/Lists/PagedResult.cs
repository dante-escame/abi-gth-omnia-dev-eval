namespace Ambev.DeveloperEvaluation.Application.Common.Lists;

public sealed record PagedResult<T>(IReadOnlyList<T> Data, int TotalItems, int CurrentPage, int TotalPages)
{
    public static PagedResult<T> From(IReadOnlyList<T> data, int totalItems, int page, int size)
    {
        int totalPages = size <= 0 ? 0 : (int)Math.Ceiling(totalItems / (double)size);
        return new PagedResult<T>(data, totalItems, page, totalPages);
    }

    public PagedResult<TOther> Map<TOther>(Func<T, TOther> selector) =>
        new(Data.Select(selector).ToList(), TotalItems, CurrentPage, TotalPages);
}
