using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Sales.ValueObjects;

public sealed class CustomerRef : ValueObject
{
    public Guid Id { get; }

    public string Name { get; }

    private CustomerRef()
    {
        Name = null!;
    }

    public CustomerRef(Guid id, string name)
    {
        if (id == Guid.Empty)
            throw new DomainException("Customer reference requires an id.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Customer reference requires a name.");

        Id = id;
        Name = name.Trim();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Id;
        yield return Name;
    }

    public override string ToString() => Name;
}
