using System.Text.Json.Serialization;
using Ambev.DeveloperEvaluation.Application.Products.Common;
using Ambev.DeveloperEvaluation.Application.Products.CreateProduct;
using Ambev.DeveloperEvaluation.Application.Products.UpdateProduct;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Products.Common;

public sealed class RatingRequest
{
    [JsonPropertyName("rate")]
    public decimal Rate { get; set; }

    [JsonPropertyName("count")]
    public int Count { get; set; }

    public RatingInput ToInput() => new(Rate, Count);
}

public sealed class ProductRequest
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("image")]
    public string Image { get; set; } = string.Empty;

    [JsonPropertyName("rating")]
    public RatingRequest Rating { get; set; } = new();

    public CreateProductCommand ToCreateCommand() => new()
    {
        Title = Title,
        Price = Price,
        Description = Description,
        Category = Category,
        Image = Image,
        Rating = Rating.ToInput()
    };

    public UpdateProductCommand ToUpdateCommand(Guid id) => new()
    {
        Id = id,
        Title = Title,
        Price = Price,
        Description = Description,
        Category = Category,
        Image = Image,
        Rating = Rating.ToInput()
    };
}
