using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Sales.ValueObjects;

public sealed class Money : ValueObject
{
    private const int Scale = 2;

    public decimal Amount { get; }

    public Money(decimal amount)
    {
        decimal rounded = decimal.Round(amount, Scale, MidpointRounding.AwayFromZero);

        if (rounded < 0)
            throw new DomainException("An amount cannot be negative.");

        Amount = rounded;
    }

    public static Money Zero => new(0m);

    public static Money operator +(Money left, Money right)
    {
        Required(left);
        Required(right);

        return new Money(left.Amount + right.Amount);
    }

    public static Money operator -(Money left, Money right)
    {
        Required(left);
        Required(right);

        return new Money(left.Amount - right.Amount);
    }

    public static Money operator *(Money value, int multiplier)
    {
        Required(value);

        return new Money(value.Amount * multiplier);
    }

    public static Money operator *(Money value, decimal multiplier)
    {
        Required(value);

        return new Money(value.Amount * multiplier);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
    }

    public override string ToString() => Amount.ToString("0.00");

    private static void Required(Money value)
    {
        if (value is null)
            throw new DomainException("An amount is required.");
    }
}
