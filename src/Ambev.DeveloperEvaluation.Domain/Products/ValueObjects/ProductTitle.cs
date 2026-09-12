using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Products.ValueObjects;

public sealed class ProductTitle : ValueObject
{
    private const int MaxLength = 200;

    public string Value { get; }

    public ProductTitle(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Product title is required.");
        if (value.Trim().Length > MaxLength)
            throw new DomainException($"Product title cannot be longer than {MaxLength} characters.");

        Value = value.Trim();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
