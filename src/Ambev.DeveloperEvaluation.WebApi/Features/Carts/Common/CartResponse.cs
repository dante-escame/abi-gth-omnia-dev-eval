using System.Text.Json.Serialization;
using Ambev.DeveloperEvaluation.Application.Carts.Common;
using Ambev.DeveloperEvaluation.Application.Common.Lists;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Carts.Common;

public sealed record CartLineResponse(
    [property: JsonPropertyName("productId")] Guid ProductId,
    [property: JsonPropertyName("title")] string? Title,
    [property: JsonPropertyName("quantity")] int Quantity);

public sealed record CartResponse(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("userId")] Guid UserId,
    [property: JsonPropertyName("date")] DateTime Date,
    [property: JsonPropertyName("products")] IReadOnlyList<CartLineResponse> Products)
{
    public static CartResponse From(CartResult cart) => new(
        cart.Id,
        cart.UserId,
        cart.Date,
        cart.Products
            .Select(line => new CartLineResponse(line.ProductId, line.Title, line.Quantity))
            .ToList());
}

public sealed record ListCartsResponse(
    [property: JsonPropertyName("data")] IReadOnlyList<CartResponse> Data,
    [property: JsonPropertyName("totalItems")] int TotalItems,
    [property: JsonPropertyName("currentPage")] int CurrentPage,
    [property: JsonPropertyName("totalPages")] int TotalPages)
{
    public static ListCartsResponse From(PagedResult<CartResult> page) => new(
        page.Data.Select(CartResponse.From).ToList(),
        page.TotalItems,
        page.CurrentPage,
        page.TotalPages);
}
