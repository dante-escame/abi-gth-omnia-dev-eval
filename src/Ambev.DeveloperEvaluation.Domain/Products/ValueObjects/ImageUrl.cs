using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Products.ValueObjects;

public sealed class ImageUrl : ValueObject
{
    public string Value { get; }

    public ImageUrl(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Product image is required.");
        if (!IsValid(value))
            throw new DomainException("Product image must be an absolute http or https URL.");

        Value = value.Trim();
    }

    public static bool IsValid(string? value) =>
        !string.IsNullOrWhiteSpace(value)
        && Uri.TryCreate(value.Trim(), UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
