using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Products.ValueObjects;

public sealed class ImageUrl : ValueObject
{
    public string Value { get; }

    public ImageUrl(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Product image is required.");
        if (!Uri.TryCreate(value.Trim(), UriKind.Absolute, out _))
            throw new DomainException("Product image must be a valid absolute URL.");

        Value = value.Trim();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
