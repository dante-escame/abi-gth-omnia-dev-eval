using System.Text.RegularExpressions;
using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Sales.ValueObjects;

public sealed partial class SaleNumber : ValueObject
{
    private const string Prefix = "SALE-";

    private const int Digits = 6;

    public string Value { get; }

    public SaleNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Sale number is required.");

        string trimmed = value.Trim();

        if (!Pattern().IsMatch(trimmed))
            throw new DomainException($"Sale number must look like {Prefix}000123.");

        Value = trimmed;
    }

    public static SaleNumber FromSequence(long sequenceValue)
    {
        if (sequenceValue < 1)
            throw new DomainException("A sale number sequence value must be positive.");

        return new SaleNumber($"{Prefix}{sequenceValue.ToString($"D{Digits}")}");
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^SALE-\d{6,}$")]
    private static partial Regex Pattern();
}
