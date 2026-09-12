using System.Text.Json.Serialization;
using Ambev.DeveloperEvaluation.Application.Common.Lists;
using Ambev.DeveloperEvaluation.Application.Sales.Common;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.Common;

public sealed record SaleItemResponse(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("productId")] Guid ProductId,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("quantity")] int Quantity,
    [property: JsonPropertyName("unitPrice")] decimal UnitPrice,
    [property: JsonPropertyName("discount")] decimal Discount,
    [property: JsonPropertyName("total")] decimal Total,
    [property: JsonPropertyName("status")] string Status);

public sealed record SaleResponse(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("saleNumber")] string SaleNumber,
    [property: JsonPropertyName("date")] DateTime Date,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("customerId")] Guid CustomerId,
    [property: JsonPropertyName("customerName")] string CustomerName,
    [property: JsonPropertyName("branchId")] Guid BranchId,
    [property: JsonPropertyName("branchName")] string BranchName,
    [property: JsonPropertyName("total")] decimal Total,
    [property: JsonPropertyName("items")] IReadOnlyList<SaleItemResponse> Items)
{
    public static SaleResponse From(SaleResult sale) => new(
        sale.Id,
        sale.SaleNumber,
        sale.Date,
        sale.Status.ToString(),
        sale.CustomerId,
        sale.CustomerName,
        sale.BranchId,
        sale.BranchName,
        sale.Total,
        sale.Items
            .Select(item => new SaleItemResponse(
                item.Id,
                item.ProductId,
                item.Title,
                item.Quantity,
                item.UnitPrice,
                item.Discount,
                item.Total,
                item.Status.ToString()))
            .ToList());
}

public sealed record ListSalesResponse(
    [property: JsonPropertyName("data")] IReadOnlyList<SaleResponse> Data,
    [property: JsonPropertyName("totalItems")] int TotalItems,
    [property: JsonPropertyName("currentPage")] int CurrentPage,
    [property: JsonPropertyName("totalPages")] int TotalPages)
{
    public static ListSalesResponse From(PagedResult<SaleResult> page) => new(
        page.Data.Select(SaleResponse.From).ToList(),
        page.TotalItems,
        page.CurrentPage,
        page.TotalPages);
}
