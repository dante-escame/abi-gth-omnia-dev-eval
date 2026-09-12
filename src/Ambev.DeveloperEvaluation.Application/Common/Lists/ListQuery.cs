namespace Ambev.DeveloperEvaluation.Application.Common.Lists;

public enum FilterOperator
{
    Equal,
    StartsWith,
    EndsWith,
    Contains,
    Min,
    Max
}

public sealed record OrderTerm(string Field, bool Descending);

public sealed record FilterTerm(string Field, FilterOperator Operator, string Value);

public sealed record ListQuery(int Page, int Size, IReadOnlyList<OrderTerm> Order, IReadOnlyList<FilterTerm> Filters)
{
    public const int DefaultPage = 1;

    public const int DefaultSize = 10;

    public static ListQuery Default => new(DefaultPage, DefaultSize, [], []);
}
