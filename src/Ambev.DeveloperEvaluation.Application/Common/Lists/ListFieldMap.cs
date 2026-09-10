using System.Linq.Expressions;

namespace Ambev.DeveloperEvaluation.Application.Common.Lists;

public sealed class ListFieldMap<T>
{
    private readonly Dictionary<string, LambdaExpression> _fields = new(StringComparer.OrdinalIgnoreCase);

    public ListFieldMap<T> Map<TValue>(string name, Expression<Func<T, TValue>> selector)
    {
        _fields[name] = selector;
        return this;
    }

    public LambdaExpression? Find(string name) =>
        _fields.TryGetValue(name, out var selector) ? selector : null;
}
