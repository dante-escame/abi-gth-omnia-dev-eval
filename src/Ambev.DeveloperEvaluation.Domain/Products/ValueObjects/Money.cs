using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Products.ValueObjects;

public sealed class Money : ValueObject
{
    public decimal Amount { get; }

    public Money(decimal amount)
    {
        if (amount <= 0)
            throw new DomainException("Price must be greater than zero.");

        Amount = amount;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
    }

    public override string ToString() => Amount.ToString("0.##");
}
