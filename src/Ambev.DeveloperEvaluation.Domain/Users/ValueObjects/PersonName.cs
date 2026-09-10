using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Users.ValueObjects;

public sealed class PersonName : ValueObject
{
    private const int MaxPartLength = 100;

    public string FirstName { get; }

    public string LastName { get; }

    public PersonName(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("First name is required.");
        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("Last name is required.");
        if (firstName.Length > MaxPartLength)
            throw new DomainException($"First name cannot be longer than {MaxPartLength} characters.");
        if (lastName.Length > MaxPartLength)
            throw new DomainException($"Last name cannot be longer than {MaxPartLength} characters.");

        FirstName = firstName;
        LastName = lastName;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return FirstName;
        yield return LastName;
    }

    public override string ToString() => $"{FirstName} {LastName}";
}
