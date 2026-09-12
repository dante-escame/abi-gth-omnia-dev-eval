using Ambev.DeveloperEvaluation.Domain.Enums;

namespace Ambev.DeveloperEvaluation.Application.Sales.Common;

public sealed class SaleListItem
{
    public Guid Id { get; set; }

    public string SaleNumber { get; set; } = string.Empty;

    public DateTime Date { get; set; }

    public SaleStatus Status { get; set; }

    public Guid CustomerId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public Guid BranchId { get; set; }

    public string BranchName { get; set; } = string.Empty;

    public decimal Total { get; set; }

    public IReadOnlyList<SaleLineItem> Items { get; set; } = [];
}

public sealed class SaleLineItem
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public string Title { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal Discount { get; set; }

    public decimal Total { get; set; }

    public SaleItemStatus Status { get; set; }
}
