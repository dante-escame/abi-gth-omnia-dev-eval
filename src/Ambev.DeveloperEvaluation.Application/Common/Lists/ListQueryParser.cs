namespace Ambev.DeveloperEvaluation.Application.Common.Lists;

public static class ListQueryParser
{
    private const string PageKey = "_page";
    private const string SizeKey = "_size";
    private const string OrderKey = "_order";
    private const string MinPrefix = "_min";
    private const string MaxPrefix = "_max";

    public static ListQuery Parse(IReadOnlyDictionary<string, string?> parameters)
    {
        var page = ReadPositiveInt(parameters, PageKey, ListQuery.DefaultPage);
        var size = ReadPositiveInt(parameters, SizeKey, ListQuery.DefaultSize);

        return new ListQuery(page, size, ParseOrder(parameters), ParseFilters(parameters));
    }

    private static int ReadPositiveInt(IReadOnlyDictionary<string, string?> parameters, string key, int fallback)
    {
        if (!parameters.TryGetValue(key, out var raw) || !int.TryParse(raw, out var value) || value < 1)
            return fallback;

        return value;
    }

    private static List<OrderTerm> ParseOrder(IReadOnlyDictionary<string, string?> parameters)
    {
        var terms = new List<OrderTerm>();

        if (!parameters.TryGetValue(OrderKey, out var raw) || string.IsNullOrWhiteSpace(raw))
            return terms;

        foreach (var segment in Unquote(raw).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var parts = segment.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length == 0)
                continue;

            var descending = parts.Length > 1 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);
            terms.Add(new OrderTerm(parts[0], descending));
        }

        return terms;
    }

    private static List<FilterTerm> ParseFilters(IReadOnlyDictionary<string, string?> parameters)
    {
        var filters = new List<FilterTerm>();

        foreach (var (key, raw) in parameters)
        {
            if (raw is null || key is PageKey or SizeKey or OrderKey)
                continue;

            if (key.StartsWith(MinPrefix, StringComparison.OrdinalIgnoreCase))
            {
                filters.Add(new FilterTerm(key[MinPrefix.Length..], FilterOperator.Min, raw));
                continue;
            }

            if (key.StartsWith(MaxPrefix, StringComparison.OrdinalIgnoreCase))
            {
                filters.Add(new FilterTerm(key[MaxPrefix.Length..], FilterOperator.Max, raw));
                continue;
            }

            if (key.StartsWith('_'))
                continue;

            filters.Add(ParseValue(key, raw));
        }

        return filters;
    }

    private static FilterTerm ParseValue(string field, string raw)
    {
        var startsWithStar = raw.StartsWith('*');
        var endsWithStar = raw.EndsWith('*');

        if (startsWithStar && endsWithStar && raw.Length > 1)
            return new FilterTerm(field, FilterOperator.Contains, raw[1..^1]);

        if (endsWithStar)
            return new FilterTerm(field, FilterOperator.StartsWith, raw[..^1]);

        if (startsWithStar)
            return new FilterTerm(field, FilterOperator.EndsWith, raw[1..]);

        return new FilterTerm(field, FilterOperator.Equal, raw);
    }

    private static string Unquote(string value)
    {
        var trimmed = value.Trim();

        return trimmed.Length > 1 && trimmed.StartsWith('"') && trimmed.EndsWith('"')
            ? trimmed[1..^1]
            : trimmed;
    }
}
