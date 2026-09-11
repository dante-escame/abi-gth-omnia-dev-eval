using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Sales;

namespace Ambev.DeveloperEvaluation.Application.Sales.Common;

public sealed record SaleItemResult(
    Guid Id,
    Guid ProductId,
    string Title,
    int Quantity,
    decimal UnitPrice,
    decimal Discount,
    decimal Total,
    SaleItemStatus Status);

public sealed record SaleResult(
    Guid Id,
    string SaleNumber,
    DateTime Date,
    SaleStatus Status,
    Guid CustomerId,
    string CustomerName,
    Guid BranchId,
    string BranchName,
    decimal Total,
    IReadOnlyList<SaleItemResult> Items)
{
    public static SaleResult From(Sale sale) => new(
        sale.Id,
        sale.Number.Value,
        sale.SoldAt,
        sale.Status,
        sale.Customer.Id,
        sale.Customer.Name,
        sale.Branch.Id,
        sale.Branch.Name,
        sale.Total.Amount,
        sale.Items
            .Select(item => new SaleItemResult(
                item.Id,
                item.Product.Id,
                item.Product.Title,
                item.Quantity.Value,
                item.UnitPrice.Amount,
                item.Totals.Discount.Amount,
                item.Totals.Net.Amount,
                item.Status))
            .ToList());

    public static SaleResult From(SaleListItem sale) => new(
        sale.Id,
        sale.SaleNumber,
        sale.Date,
        sale.Status,
        sale.CustomerId,
        sale.CustomerName,
        sale.BranchId,
        sale.BranchName,
        sale.Total,
        sale.Items
            .Select(item => new SaleItemResult(
                item.Id,
                item.ProductId,
                item.Title,
                item.Quantity,
                item.UnitPrice,
                item.Discount,
                item.Total,
                item.Status))
            .ToList());
}
