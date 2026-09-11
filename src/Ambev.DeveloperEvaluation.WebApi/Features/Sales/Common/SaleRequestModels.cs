using System.Text.Json.Serialization;
using Ambev.DeveloperEvaluation.Application.Common;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.Common;

public sealed class CreateSaleRequest
{
    [JsonPropertyName("cartId")]
    public Guid CartId { get; set; }

    [JsonPropertyName("branchId")]
    public Guid BranchId { get; set; }

    public CreateSaleCommand ToCommand(CallerContext caller) => new(caller, CartId, BranchId);
}

public sealed class SaleLineRequest
{
    [JsonPropertyName("productId")]
    public Guid ProductId { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    public SaleLineInput ToInput() => new(ProductId, Quantity);
}

public sealed class UpdateSaleRequest
{
    [JsonPropertyName("items")]
    public List<SaleLineRequest> Items { get; set; } = [];

    public UpdateSaleCommand ToCommand(Guid id, CallerContext caller) =>
        new(id, caller, Items.Select(line => line.ToInput()).ToList());
}

public sealed class CancelSaleRequest
{
    [JsonPropertyName("reason")]
    public string? Reason { get; set; }

    public CancelSaleCommand ToCommand(Guid id, CallerContext caller) => new(id, caller, Reason);
}
