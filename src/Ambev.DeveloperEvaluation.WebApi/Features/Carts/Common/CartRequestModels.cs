using System.Text.Json.Serialization;
using Ambev.DeveloperEvaluation.Application.Carts.Common;
using Ambev.DeveloperEvaluation.Application.Carts.CreateCart;
using Ambev.DeveloperEvaluation.Application.Carts.UpdateCart;
using Ambev.DeveloperEvaluation.Application.Common;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Carts.Common;

public sealed class CartLineRequest
{
    [JsonPropertyName("productId")]
    public Guid ProductId { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    public CartLineInput ToInput() => new(ProductId, Quantity);
}

public sealed class CartRequest
{
    [JsonPropertyName("userId")]
    public Guid UserId { get; set; }

    [JsonPropertyName("date")]
    public DateTime? Date { get; set; }

    [JsonPropertyName("products")]
    public List<CartLineRequest> Products { get; set; } = [];

    public CreateCartCommand ToCreateCommand(CallerContext caller) => new(caller, ToInputs());

    public UpdateCartCommand ToUpdateCommand(Guid id, CallerContext caller) => new(id, caller, ToInputs());

    private List<CartLineInput> ToInputs() => Products.Select(line => line.ToInput()).ToList();
}
