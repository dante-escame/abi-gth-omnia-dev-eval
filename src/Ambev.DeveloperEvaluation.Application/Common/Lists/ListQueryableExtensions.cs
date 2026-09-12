using System.Linq.Expressions;
using System.Reflection;

namespace Ambev.DeveloperEvaluation.Application.Common.Lists;

public static class ListQueryableExtensions
{
    private static readonly MethodInfo StartsWith =
        typeof(string).GetMethod(nameof(string.StartsWith), [typeof(string)])!;

    private static readonly MethodInfo EndsWith =
        typeof(string).GetMethod(nameof(string.EndsWith), [typeof(string)])!;

    private static readonly MethodInfo Contains =
        typeof(string).GetMethod(nameof(string.Contains), [typeof(string)])!;

    public static IQueryable<T> ApplyFilters<T>(this IQueryable<T> source, ListQuery query, ListFieldMap<T> map)
    {
        foreach (var filter in query.Filters)
        {
            var selector = map.Find(filter.Field);
            if (selector is null)
                continue;

            var predicate = BuildPredicate<T>(selector, filter);
            if (predicate is not null)
                source = source.Where(predicate);
        }

        return source;
    }

    public static IQueryable<T> ApplyOrdering<T>(this IQueryable<T> source, ListQuery query, ListFieldMap<T> map)
    {
        var ordered = false;

        foreach (var term in query.Order)
        {
            var selector = map.Find(term.Field);
            if (selector is null)
                continue;

            var method = (ordered, term.Descending) switch
            {
                (false, false) => nameof(Queryable.OrderBy),
                (false, true) => nameof(Queryable.OrderByDescending),
                (true, false) => nameof(Queryable.ThenBy),
                (true, true) => nameof(Queryable.ThenByDescending)
            };

            var call = Expression.Call(
                typeof(Queryable),
                method,
                [typeof(T), selector.ReturnType],
                source.Expression,
                Expression.Quote(selector));

            source = source.Provider.CreateQuery<T>(call);
            ordered = true;
        }

        return source;
    }

    private static Expression<Func<T, bool>>? BuildPredicate<T>(LambdaExpression selector, FilterTerm filter)
    {
        var member = selector.Body;

        Expression? body = filter.Operator switch
        {
            FilterOperator.StartsWith when member.Type == typeof(string) =>
                Expression.Call(member, StartsWith, Expression.Constant(filter.Value)),
            FilterOperator.EndsWith when member.Type == typeof(string) =>
                Expression.Call(member, EndsWith, Expression.Constant(filter.Value)),
            FilterOperator.Contains when member.Type == typeof(string) =>
                Expression.Call(member, Contains, Expression.Constant(filter.Value)),
            FilterOperator.Min => Compare(member, filter.Value, Expression.GreaterThanOrEqual),
            FilterOperator.Max => Compare(member, filter.Value, Expression.LessThanOrEqual),
            _ => Compare(member, filter.Value, Expression.Equal)
        };

        return body is null
            ? null
            : Expression.Lambda<Func<T, bool>>(body, selector.Parameters[0]);
    }

    private static Expression? Compare(
        Expression member,
        string raw,
        Func<Expression, Expression, BinaryExpression> factory)
    {
        var value = ConvertValue(member.Type, raw);

        return value is null ? null : factory(member, Expression.Constant(value, member.Type));
    }

    private static object? ConvertValue(Type target, string raw)
    {
        try
        {
            var underlying = Nullable.GetUnderlyingType(target) ?? target;

            if (underlying == typeof(string))
                return raw;

            if (underlying.IsEnum)
                return Enum.Parse(underlying, raw, ignoreCase: true);

            if (underlying == typeof(Guid))
                return Guid.Parse(raw);

            return Convert.ChangeType(raw, underlying);
        }
        catch (Exception exception) when (exception is FormatException or ArgumentException or InvalidCastException or OverflowException)
        {
            return null;
        }
    }
}
