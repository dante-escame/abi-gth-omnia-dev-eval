using Ambev.DeveloperEvaluation.Domain.Carts.ValueObjects;
using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Enums;

namespace Ambev.DeveloperEvaluation.Domain.Carts;

public class Cart : AggregateRoot
{
    private readonly List<CartItem> _items = [];

    public Guid CustomerId { get; private set; }

    public CartStatus Status { get; private set; }

    public Guid? SaleId { get; private set; }

    public DateTime? CheckedOutAt { get; private set; }

    public IReadOnlyList<CartItem> Items => _items.AsReadOnly();

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    private Cart()
    {
    }

    public static Cart Create(Guid customerId, IEnumerable<CartItem> items)
    {
        var cart = new Cart
        {
            Id = Guid.NewGuid(),
            CustomerId = RequiredCustomer(customerId),
            Status = CartStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        cart._items.AddRange(Merge(items));

        return cart;
    }

    public static Cart Restore(
        Guid id,
        Guid customerId,
        CartStatus status,
        IEnumerable<CartItem> items,
        DateTime createdAt,
        DateTime? updatedAt)
    {
        var cart = new Cart
        {
            Id = id,
            CustomerId = RequiredCustomer(customerId),
            Status = status,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        cart._items.AddRange(Merge(items));

        return cart;
    }

    public void MarkCheckedOut(Guid saleId)
    {
        if (Status == CartStatus.CheckedOut)
            return;

        if (saleId == Guid.Empty)
            throw new DomainException("A checkout requires a sale.");

        Status = CartStatus.CheckedOut;
        SaleId = saleId;
        CheckedOutAt = DateTime.UtcNow;
        Touch();
    }

    public void ReplaceItems(IEnumerable<CartItem> items)
    {
        EnsureActive();

        var merged = Merge(items);

        _items.Clear();
        _items.AddRange(merged);
        Touch();
    }

    private void EnsureActive()
    {
        if (Status != CartStatus.Active)
            throw new DomainException($"A {Status} cart cannot be modified.");
    }

    private void Touch() => UpdatedAt = DateTime.UtcNow;

    private static Guid RequiredCustomer(Guid customerId) =>
        customerId == Guid.Empty ? throw new DomainException("Cart requires a customer.") : customerId;

    private static List<CartItem> Merge(IEnumerable<CartItem> items)
    {
        if (items is null)
            throw new DomainException("Cart requires an item list.");

        var merged = new List<CartItem>();

        foreach (var item in items)
        {
            if (item is null)
                throw new DomainException("A cart item cannot be empty.");

            int index = merged.FindIndex(existing => existing.Product.Id == item.Product.Id);

            if (index < 0)
            {
                merged.Add(item);
            }
            else
            {
                merged[index] = merged[index]
                    .WithQuantity(merged[index].Quantity.Add(item.Quantity));
            }
        }
        return merged;
    }
}
