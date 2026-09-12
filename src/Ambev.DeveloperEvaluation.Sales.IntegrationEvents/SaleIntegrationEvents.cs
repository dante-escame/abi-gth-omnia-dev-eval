namespace Ambev.DeveloperEvaluation.Sales.IntegrationEvents;

public sealed record SoldItem(
    Guid ItemId,
    Guid ProductId,
    string Title,
    int Quantity,
    decimal UnitPrice,
    decimal Discount,
    decimal Total);

public sealed record SaleCreatedIntegrationEvent(
    Guid SaleId,
    string SaleNumber,
    Guid CartId,
    Guid CustomerId,
    string CustomerName,
    Guid BranchId,
    string BranchName,
    IReadOnlyList<SoldItem> Items,
    decimal Total,
    DateTime SoldAt,
    DateTime OccurredOnUtc);

public sealed record SaleModifiedIntegrationEvent(
    Guid SaleId,
    string SaleNumber,
    IReadOnlyList<SoldItem> Items,
    decimal Total,
    DateTime ModifiedAt,
    DateTime OccurredOnUtc);

public sealed record SaleCancelledIntegrationEvent(
    Guid SaleId,
    string SaleNumber,
    string? Reason,
    DateTime CancelledAt,
    DateTime OccurredOnUtc);

public sealed record ItemCancelledIntegrationEvent(
    Guid SaleId,
    string SaleNumber,
    Guid ItemId,
    Guid ProductId,
    string Title,
    decimal NewSaleTotal,
    DateTime CancelledAt,
    DateTime OccurredOnUtc);
