using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Validation;

namespace Ambev.DeveloperEvaluation.Domain.Users.ValueObjects;

public sealed class Email : ValueObject
{
    private static readonly EmailValidator Validator = new();

    public string Value { get; }

    public Email(string value)
    {
        var result = Validator.Validate(value ?? string.Empty);
        if (!result.IsValid)
            throw new DomainException(result.Errors[0].ErrorMessage);

        Value = value!;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
