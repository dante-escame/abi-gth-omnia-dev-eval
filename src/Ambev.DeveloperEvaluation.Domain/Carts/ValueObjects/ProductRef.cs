using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Carts.ValueObjects;

public sealed class ProductRef : ValueObject
{
    public Guid Id { get; }

    public string? Title { get; }

    public ProductRef(Guid id, string? title = null)
    {
        if (id == Guid.Empty)
            throw new DomainException("Product reference requires an id.");

        Id = id;
        Title = string.IsNullOrWhiteSpace(title) ? null : title.Trim();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Id;
        yield return Title;
    }

    public override string ToString() => Title ?? Id.ToString();
}
