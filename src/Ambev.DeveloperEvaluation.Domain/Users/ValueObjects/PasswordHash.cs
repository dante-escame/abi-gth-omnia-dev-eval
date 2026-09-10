using System.Text.RegularExpressions;
using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Users.ValueObjects;

public sealed partial class PasswordHash : ValueObject
{
    private static readonly Regex BcryptPattern = MyRegex();

    public string Value { get; }

    public PasswordHash(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !BcryptPattern.IsMatch(value))
            throw new DomainException("Password must be a valid BCrypt hash.");

        Value = value;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
    
    [GeneratedRegex(@"^\$2[abxy]?\$\d{2}\$[./A-Za-z0-9]{53}$", RegexOptions.Compiled)]
    private static partial Regex MyRegex();
}
