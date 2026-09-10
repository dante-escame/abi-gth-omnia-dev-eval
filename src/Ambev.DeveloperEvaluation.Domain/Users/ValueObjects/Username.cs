using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Users.ValueObjects;

public sealed class Username : ValueObject
{
    public string Value { get; }

    public Username(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Username cannot be empty.");
        
        if (value.Length < 3)
            throw new DomainException("Username must be at least 3 characters long.");
        
        if (value.Length > 50)
            throw new DomainException("Username cannot be longer than 50 characters.");

        Value = value;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
