using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Sales.Events;

public sealed record SaleEventItem(
    Guid ItemId,
    Guid ProductId,
    string Title,
    int Quantity,
    decimal UnitPrice,
    decimal Discount,
    decimal Total);

public sealed record SaleCreatedDomainEvent(
    Guid SaleId,
    string SaleNumber,
    Guid CartId,
    Guid CustomerId,
    string CustomerName,
    Guid BranchId,
    string BranchName,
    IReadOnlyList<SaleEventItem> Items,
    decimal Total,
    DateTime CreatedAt) : IDomainEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
}

public sealed record SaleModifiedDomainEvent(
    Guid SaleId,
    string SaleNumber,
    IReadOnlyList<SaleEventItem> Items,
    decimal Total,
    DateTime ModifiedAt) : IDomainEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
}

public sealed record SaleCancelledDomainEvent(
    Guid SaleId,
    string SaleNumber,
    string? Reason,
    DateTime CancelledAt) : IDomainEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
}

public sealed record ItemCancelledDomainEvent(
    Guid SaleId,
    Guid ItemId,
    Guid ProductId,
    string Title,
    decimal NewSaleTotal,
    DateTime CancelledAt) : IDomainEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
}
