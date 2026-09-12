using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Products.ValueObjects;

public sealed class Rating : ValueObject
{
    private const decimal MinRate = 0.0m;

    private const decimal MaxRate = 5.0m;

    public decimal Rate { get; }

    public int Count { get; }

    public Rating(decimal rate, int count)
    {
        if (rate < MinRate || rate > MaxRate)
            throw new DomainException($"Rating rate must be between {MinRate:0.0} and {MaxRate:0.0}.");
        if (count < 0)
            throw new DomainException("Rating count cannot be negative.");

        Rate = rate;
        Count = count;
    }

    public static Rating None => new(MinRate, 0);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Rate;
        yield return Count;
    }
}
