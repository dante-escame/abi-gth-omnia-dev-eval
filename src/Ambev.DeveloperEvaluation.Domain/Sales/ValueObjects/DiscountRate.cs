using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Sales.ValueObjects;

public sealed class DiscountRate : ValueObject
{
    private DiscountRate(decimal value)
    {
        Value = value;
    }

    public decimal Value { get; }

    public static DiscountRate None { get; } = new(0m);

    public static DiscountRate TenPercent { get; } = new(0.10m);

    public static DiscountRate TwentyPercent { get; } = new(0.20m);

    public static DiscountRate FromValue(decimal value) =>
        Allowed().FirstOrDefault(rate => rate.Value == value)
        ?? throw new DomainException($"{value} is not a supported discount rate.");

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString("0.00");

    private static IEnumerable<DiscountRate> Allowed()
    {
        yield return None;
        yield return TenPercent;
        yield return TwentyPercent;
    }
}
