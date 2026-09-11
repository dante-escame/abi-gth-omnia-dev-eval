using Ambev.DeveloperEvaluation.Domain.Products;

namespace Ambev.DeveloperEvaluation.Application.Products.Common;

public sealed record RatingResult(decimal Rate, int Count);

public sealed record ProductResult(
    Guid Id,
    string Title,
    decimal Price,
    string Description,
    string Category,
    string Image,
    RatingResult Rating)
{
    public static ProductResult From(Product product) => new(
        product.Id,
        product.Title.Value,
        product.Price.Amount,
        product.Description,
        product.Category.Name,
        product.Image.Value,
        new RatingResult(product.Rating.Rate, product.Rating.Count));

    public static ProductResult From(ProductListItem item) => new(
        item.Id,
        item.Title,
        item.Price,
        item.Description,
        item.Category,
        item.Image,
        new RatingResult(item.Rate, item.RatingCount));
}
