using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Sales.ValueObjects;

public sealed class Quantity : ValueObject
{
    public const int Minimum = 1;

    public const int Maximum = 20;

    public int Value { get; }

    public Quantity(int value)
    {
        if (value < Minimum)
            throw new DomainException($"Quantity must be at least {Minimum}.");

        if (value > Maximum)
            throw new MaxItemsExceededException(value, Maximum);

        Value = value;
    }

    public Quantity Add(Quantity other)
    {
        if (other is null)
            throw new DomainException("Quantity to add is required.");

        return new Quantity(Value + other.Value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}
