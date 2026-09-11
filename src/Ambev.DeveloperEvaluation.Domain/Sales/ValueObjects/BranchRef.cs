using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Sales.ValueObjects;

public sealed class BranchRef : ValueObject
{
    public Guid Id { get; }

    public string Name { get; }

    private BranchRef()
    {
        Name = null!;
    }

    public BranchRef(Guid id, string name)
    {
        if (id == Guid.Empty)
            throw new DomainException("Branch reference requires an id.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Branch reference requires a name.");

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
