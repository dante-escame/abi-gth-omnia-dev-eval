using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Sales.Events;
using Ambev.DeveloperEvaluation.Domain.Sales.Services;
using Ambev.DeveloperEvaluation.Domain.Sales.ValueObjects;

namespace Ambev.DeveloperEvaluation.Domain.Sales;

public class Sale : AggregateRoot
{
    public const string DeletedReason = "The sale was deleted.";

    private readonly List<SaleItem> _items = [];

    public SaleNumber Number { get; private set; } = null!;

    public DateTime SoldAt { get; private set; }

    public CustomerRef Customer { get; private set; } = null!;

    public BranchRef Branch { get; private set; } = null!;

    public Guid CartId { get; private set; }

    public SaleStatus Status { get; private set; }

    public Money Total { get; private set; } = null!;

    public bool IsDeleted { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? ModifiedAt { get; private set; }

    public DateTime? CancelledAt { get; private set; }

    public IReadOnlyList<SaleItem> Items => _items.AsReadOnly();

    private Sale()
    {
    }

    public static Sale Create(
        SaleNumber number,
        CustomerRef customer,
        BranchRef branch,
        Guid cartId,
        IEnumerable<SaleLine> lines,
        IDiscountPolicy policy)
    {
        var now = DateTime.UtcNow;

        var sale = new Sale
        {
            Id = Guid.NewGuid(),
            Number = number ?? throw new DomainException("A sale requires a number."),
            Customer = customer ?? throw new DomainException("A sale requires a customer."),
            Branch = branch ?? throw new DomainException("A sale requires a branch."),
            CartId = cartId == Guid.Empty ? throw new DomainException("A sale requires a cart.") : cartId,
            Status = SaleStatus.Active,
            SoldAt = now,
            CreatedAt = now
        };

        foreach (var line in Merge(lines))
            sale._items.Add(SaleItem.Create(line, RequiredPolicy(policy)));

        if (sale._items.Count == 0)
            throw new DomainException("A sale requires at least one item.");

        sale.Recalculate();

        sale.Raise(new SaleCreatedDomainEvent(
            sale.Id,
            sale.Number.Value,
            sale.CartId,
            sale.Customer.Id,
            sale.Customer.Name,
            sale.Branch.Id,
            sale.Branch.Name,
            sale.ActiveEventItems(),
            sale.Total.Amount,
            sale.CreatedAt));

        return sale;
    }

    public static Sale Restore(
        Guid id,
        SaleNumber number,
        DateTime soldAt,
        CustomerRef customer,
        BranchRef branch,
        Guid cartId,
        SaleStatus status,
        Money total,
        bool isDeleted,
        IEnumerable<SaleItem> items,
        DateTime createdAt,
        DateTime? modifiedAt,
        DateTime? cancelledAt)
    {
        var sale = new Sale
        {
            Id = id,
            Number = number ?? throw new DomainException("A sale requires a number."),
            SoldAt = soldAt,
            Customer = customer ?? throw new DomainException("A sale requires a customer."),
            Branch = branch ?? throw new DomainException("A sale requires a branch."),
            CartId = cartId,
            Status = status,
            Total = total ?? throw new DomainException("A sale requires a total."),
            IsDeleted = isDeleted,
            CreatedAt = createdAt,
            ModifiedAt = modifiedAt,
            CancelledAt = cancelledAt
        };

        sale._items.AddRange(items ?? throw new DomainException("A sale requires an item list."));

        return sale;
    }

    public void ModifyItems(IEnumerable<SaleLineChange> changes, IDiscountPolicy policy)
    {
        EnsureActive();
        RequiredPolicy(policy);

        var merged = MergeChanges(changes);

        if (merged.Count == 0)
            throw new DomainException("A sale requires at least one item.");

        foreach (var change in merged)
        {
            var item = _items.FirstOrDefault(candidate =>
                candidate.IsActive && candidate.Product.Id == change.ProductId);

            if (item is null)
                throw new DomainException($"Product {change.ProductId} is not on this sale.");

            item.Reprice(change.Quantity, policy);
        }

        _items.RemoveAll(item =>
            item.IsActive && merged.All(change => change.ProductId != item.Product.Id));

        ModifiedAt = DateTime.UtcNow;
        Recalculate();

        Raise(new SaleModifiedDomainEvent(
            Id,
            Number.Value,
            ActiveEventItems(),
            Total.Amount,
            ModifiedAt.Value));
    }

    public void Cancel(string? reason = null)
    {
        if (Status == SaleStatus.Cancelled)
            return;

        foreach (var item in _items.Where(candidate => candidate.IsActive))
            item.Cancel();

        Status = SaleStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        Recalculate();

        Raise(new SaleCancelledDomainEvent(Id, Number.Value, Normalize(reason), CancelledAt.Value));
    }

    public void Delete()
    {
        if (IsDeleted)
            return;

        Cancel(DeletedReason);
        IsDeleted = true;
    }

    public void CancelItem(Guid itemId)
    {
        EnsureActive();

        var item = _items.FirstOrDefault(candidate => candidate.Id == itemId)
            ?? throw new DomainException($"Item {itemId} is not on this sale.");

        item.Cancel();
        Recalculate();

        Raise(new ItemCancelledDomainEvent(
            Id,
            Number.Value,
            item.Id,
            item.Product.Id,
            item.Product.Title,
            Total.Amount,
            DateTime.UtcNow));

        if (_items.Any(candidate => candidate.IsActive))
            return;

        Cancel("The last active item was cancelled.");
    }

    private void Recalculate()
    {
        var total = Money.Zero;

        foreach (var item in _items.Where(candidate => candidate.IsActive))
            total += item.Totals.Net;

        Total = total;
    }

    private void EnsureActive()
    {
        if (Status != SaleStatus.Active)
            throw new DomainException("A cancelled sale cannot be changed.");
    }

    private IReadOnlyList<SaleEventItem> ActiveEventItems() =>
        _items
            .Where(item => item.IsActive)
            .Select(item => new SaleEventItem(
                item.Id,
                item.Product.Id,
                item.Product.Title,
                item.Quantity.Value,
                item.UnitPrice.Amount,
                item.Totals.Discount.Amount,
                item.Totals.Net.Amount))
            .ToList();

    private static string? Normalize(string? reason) =>
        string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();

    private static IDiscountPolicy RequiredPolicy(IDiscountPolicy policy) =>
        policy ?? throw new DomainException("A sale requires a discount policy.");

    private static List<SaleLine> Merge(IEnumerable<SaleLine> lines)
    {
        if (lines is null)
            throw new DomainException("A sale requires a line list.");

        var merged = new List<SaleLine>();

        foreach (var line in lines)
        {
            if (line is null)
                throw new DomainException("A sale line cannot be empty.");

            if (line.Product is null)
                throw new DomainException("A sale line requires a product.");

            if (line.Quantity is null)
                throw new DomainException("A sale line requires a quantity.");

            int index = merged.FindIndex(existing => existing.Product.Id == line.Product.Id);

            if (index < 0)
                merged.Add(line);
            else
                merged[index] = merged[index] with { Quantity = merged[index].Quantity.Add(line.Quantity) };
        }

        return merged;
    }

    private static List<SaleLineChange> MergeChanges(IEnumerable<SaleLineChange> changes)
    {
        if (changes is null)
            throw new DomainException("A sale requires a line list.");

        var merged = new List<SaleLineChange>();

        foreach (var change in changes)
        {
            if (change is null)
                throw new DomainException("A sale line cannot be empty.");

            if (change.Quantity is null)
                throw new DomainException("A sale line requires a quantity.");

            int index = merged.FindIndex(existing => existing.ProductId == change.ProductId);

            if (index < 0)
                merged.Add(change);
            else
                merged[index] = merged[index] with { Quantity = merged[index].Quantity.Add(change.Quantity) };
        }

        return merged;
    }
}
