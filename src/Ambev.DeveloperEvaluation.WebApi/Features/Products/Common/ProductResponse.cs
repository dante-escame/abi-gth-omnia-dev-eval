using System.Text.Json.Serialization;
using Ambev.DeveloperEvaluation.Application.Common.Lists;
using Ambev.DeveloperEvaluation.Application.Products.Common;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Products.Common;

public sealed record RatingResponse(
    [property: JsonPropertyName("rate")] decimal Rate,
    [property: JsonPropertyName("count")] int Count);

public sealed record ProductResponse(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("price")] decimal Price,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("category")] string Category,
    [property: JsonPropertyName("image")] string Image,
    [property: JsonPropertyName("rating")] RatingResponse Rating)
{
    public static ProductResponse From(ProductResult product) => new(
        product.Id,
        product.Title,
        product.Price,
        product.Description,
        product.Category,
        product.Image,
        new RatingResponse(product.Rating.Rate, product.Rating.Count));
}

public sealed record ListProductsResponse(
    [property: JsonPropertyName("data")] IReadOnlyList<ProductResponse> Data,
    [property: JsonPropertyName("totalItems")] int TotalItems,
    [property: JsonPropertyName("currentPage")] int CurrentPage,
    [property: JsonPropertyName("totalPages")] int TotalPages)
{
    public static ListProductsResponse From(PagedResult<ProductResult> page) => new(
        page.Data.Select(ProductResponse.From).ToList(),
        page.TotalItems,
        page.CurrentPage,
        page.TotalPages);
}
