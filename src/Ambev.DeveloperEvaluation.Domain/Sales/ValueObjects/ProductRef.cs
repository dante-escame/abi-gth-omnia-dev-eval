using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Sales.ValueObjects;

public sealed class ProductRef : ValueObject
{
    public Guid Id { get; }

    public string Title { get; }

    private ProductRef()
    {
        Title = null!;
    }

    public ProductRef(Guid id, string title)
    {
        if (id == Guid.Empty)
            throw new DomainException("Product reference requires an id.");

        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Product reference requires a title.");

        Id = id;
        Title = title.Trim();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Id;
        yield return Title;
    }

    public override string ToString() => Title;
}
