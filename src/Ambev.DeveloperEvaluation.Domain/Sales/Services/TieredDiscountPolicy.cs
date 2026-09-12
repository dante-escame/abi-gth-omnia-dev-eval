using Ambev.DeveloperEvaluation.Domain.Sales.ValueObjects;

namespace Ambev.DeveloperEvaluation.Domain.Sales.Services;

public sealed class TieredDiscountPolicy : IDiscountPolicy
{
    private const int TenPercentThreshold = 4;

    private const int TwentyPercentThreshold = 10;

    public DiscountRate Resolve(Quantity quantity)
    {
        if (quantity is null)
            throw new DomainException("A discount rate requires a quantity.");

        if (quantity.Value >= TwentyPercentThreshold)
            return DiscountRate.TwentyPercent;

        if (quantity.Value >= TenPercentThreshold)
            return DiscountRate.TenPercent;

        return DiscountRate.None;
    }
}
