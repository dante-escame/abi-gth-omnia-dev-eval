using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Products.ValueObjects;

public sealed class Category : ValueObject
{
    private const int MaxLength = 100;

    public string Name { get; }

    public Category(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Category is required.");
        if (name.Trim().Length > MaxLength)
            throw new DomainException($"Category cannot be longer than {MaxLength} characters.");

        Name = name.Trim();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Name.ToLowerInvariant();
    }

    public override string ToString() => Name;
}
