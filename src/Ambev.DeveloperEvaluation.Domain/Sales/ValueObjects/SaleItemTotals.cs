using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Sales.ValueObjects;

public sealed class SaleItemTotals : ValueObject
{
    public Money Gross { get; }

    public Money Discount { get; }

    public Money Net { get; }

    private SaleItemTotals()
    {
        Gross = null!;
        Discount = null!;
        Net = null!;
    }

    public SaleItemTotals(Money gross, Money discount, Money net)
    {
        Gross = Required(gross, nameof(gross));
        Discount = Required(discount, nameof(discount));
        Net = Required(net, nameof(net));

        if (Discount.Amount > Gross.Amount)
            throw new DomainException("A discount cannot be larger than the gross amount.");

        if (Net != Gross - Discount)
            throw new DomainException("A net amount must equal the gross amount minus the discount.");
    }

    public static SaleItemTotals From(Money unitPrice, Quantity quantity, DiscountRate rate)
    {
        Required(unitPrice, nameof(unitPrice));

        if (quantity is null)
            throw new DomainException("Sale item totals require a quantity.");

        if (rate is null)
            throw new DomainException("Sale item totals require a discount rate.");

        var gross = unitPrice * quantity.Value;
        var discount = gross * rate.Value;

        return new SaleItemTotals(gross, discount, gross - discount);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Gross;
        yield return Discount;
        yield return Net;
    }

    private static Money Required(Money value, string name) =>
        value ?? throw new DomainException($"Sale item totals require a {name} amount.");
}
