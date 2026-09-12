using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Carts.ValueObjects;

public sealed class Quantity : ValueObject
{
    private const int Minimum = 1;

    public int Value { get; }

    public Quantity(int value)
    {
        if (value < Minimum)
            throw new DomainException($"Quantity must be at least {Minimum}.");

        Value = value;
    }

    public Quantity Add(Quantity other)
    {
        if (other is null)
            throw new DomainException("Quantity to add is required.");

        long sum = (long)Value + other.Value;

        if (sum > int.MaxValue)
            throw new DomainException("Quantity is too large.");

        return new Quantity((int)sum);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}
