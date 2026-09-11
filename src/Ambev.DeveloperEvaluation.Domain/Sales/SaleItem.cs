using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Sales.Services;
using Ambev.DeveloperEvaluation.Domain.Sales.ValueObjects;

namespace Ambev.DeveloperEvaluation.Domain.Sales;

public class SaleItem : BaseEntity
{
    public ProductRef Product { get; private set; } = null!;

    public Quantity Quantity { get; private set; } = null!;

    public Money UnitPrice { get; private set; } = null!;

    public DiscountRate Rate { get; private set; } = null!;

    public SaleItemTotals Totals { get; private set; } = null!;

    public SaleItemStatus Status { get; private set; }

    private SaleItem()
    {
    }

    internal static SaleItem Create(SaleLine line, IDiscountPolicy policy)
    {
        if (line is null)
            throw new DomainException("A sale item requires a line.");

        var item = new SaleItem
        {
            Id = Guid.NewGuid(),
            Product = Required(line.Product, "product"),
            UnitPrice = line.UnitPrice ?? throw new DomainException("A sale item requires a unit price."),
            Status = SaleItemStatus.Active
        };

        item.Reprice(line.Quantity, policy);

        return item;
    }

    internal static SaleItem Restore(
        Guid id,
        ProductRef product,
        Quantity quantity,
        Money unitPrice,
        DiscountRate rate,
        SaleItemTotals totals,
        SaleItemStatus status)
    {
        return new SaleItem
        {
            Id = id,
            Product = Required(product, "product"),
            Quantity = quantity ?? throw new DomainException("A sale item requires a quantity."),
            UnitPrice = unitPrice ?? throw new DomainException("A sale item requires a unit price."),
            Rate = rate ?? throw new DomainException("A sale item requires a discount rate."),
            Totals = totals ?? throw new DomainException("A sale item requires its totals."),
            Status = status
        };
    }

    public bool IsActive => Status == SaleItemStatus.Active;

    internal void Reprice(Quantity quantity, IDiscountPolicy policy)
    {
        if (quantity is null)
            throw new DomainException("A sale item requires a quantity.");

        if (policy is null)
            throw new DomainException("A sale item requires a discount policy.");

        Quantity = quantity;
        Rate = policy.Resolve(quantity);
        Totals = SaleItemTotals.From(UnitPrice, Quantity, Rate);
    }

    internal void Cancel()
    {
        if (Status == SaleItemStatus.Cancelled)
            throw new DomainException("This sale item is already cancelled.");

        Status = SaleItemStatus.Cancelled;
    }

    private static ProductRef Required(ProductRef product, string name) =>
        product ?? throw new DomainException($"A sale item requires a {name}.");
}
