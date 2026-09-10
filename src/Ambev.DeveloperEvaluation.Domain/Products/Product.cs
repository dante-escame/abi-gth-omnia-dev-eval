using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Products.ValueObjects;

namespace Ambev.DeveloperEvaluation.Domain.Products;

public class Product : AggregateRoot
{
    public ProductTitle Title { get; private set; } = null!;

    public Money Price { get; private set; } = null!;

    public string Description { get; private set; } = string.Empty;

    public Category Category { get; private set; } = null!;

    public ImageUrl Image { get; private set; } = null!;

    public Rating Rating { get; private set; } = null!;

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    private Product()
    {
    }

    public static Product Create(
        ProductTitle title,
        Money price,
        string? description,
        Category category,
        ImageUrl image,
        Rating rating)
    {
        return new Product
        {
            Id = Guid.NewGuid(),
            Title = Required(title, nameof(title)),
            Price = Required(price, nameof(price)),
            Description = Normalize(description),
            Category = Required(category, nameof(category)),
            Image = Required(image, nameof(image)),
            Rating = Required(rating, nameof(rating)),
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Product Restore(
        Guid id,
        ProductTitle title,
        Money price,
        string? description,
        Category category,
        ImageUrl image,
        Rating rating,
        DateTime createdAt,
        DateTime? updatedAt)
    {
        return new Product
        {
            Id = id,
            Title = Required(title, nameof(title)),
            Price = Required(price, nameof(price)),
            Description = Normalize(description),
            Category = Required(category, nameof(category)),
            Image = Required(image, nameof(image)),
            Rating = Required(rating, nameof(rating)),
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
    }

    public void UpdateDetails(ProductTitle title, string? description, Category category, ImageUrl image)
    {
        Title = Required(title, nameof(title));
        Description = Normalize(description);
        Category = Required(category, nameof(category));
        Image = Required(image, nameof(image));
        Touch();
    }

    public void Reprice(Money price)
    {
        Price = Required(price, nameof(price));
        Touch();
    }

    public void SetRating(Rating rating)
    {
        Rating = Required(rating, nameof(rating));
        Touch();
    }

    private void Touch() => UpdatedAt = DateTime.UtcNow;

    private static string Normalize(string? description) =>
        string.IsNullOrWhiteSpace(description) ? string.Empty : description.Trim();

    private static T Required<T>(T value, string name) where T : ValueObject =>
        value ?? throw new DomainException($"Product {name} is required.");
}
