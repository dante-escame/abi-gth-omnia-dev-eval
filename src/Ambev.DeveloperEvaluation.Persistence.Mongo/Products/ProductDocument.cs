using Ambev.DeveloperEvaluation.Domain.Products;
using Ambev.DeveloperEvaluation.Domain.Products.ValueObjects;
using Ambev.DeveloperEvaluation.Persistence.Mongo.Outbox;
using MongoDB.Bson.Serialization.Attributes;

namespace Ambev.DeveloperEvaluation.Persistence.Mongo.Products;

public sealed class ProductDocument
{
    [BsonId]
    public Guid Id { get; set; }

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("price")]
    public decimal Price { get; set; }

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("category")]
    public string Category { get; set; } = string.Empty;

    [BsonElement("image")]
    public string Image { get; set; } = string.Empty;

    [BsonElement("rate")]
    public decimal Rate { get; set; }

    [BsonElement("ratingCount")]
    public int RatingCount { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    public DateTime? UpdatedAt { get; set; }

    [BsonElement("deletedAt")]
    public DateTime? DeletedAt { get; set; }

    [BsonElement("pendingEvents")]
    public List<PendingEventDocument> PendingEvents { get; set; } = [];

    public static ProductDocument From(Product product) => new()
    {
        Id = product.Id,
        Title = product.Title.Value,
        Price = product.Price.Amount,
        Description = product.Description,
        Category = product.Category.Name,
        Image = product.Image.Value,
        Rate = product.Rating.Rate,
        RatingCount = product.Rating.Count,
        CreatedAt = product.CreatedAt,
        UpdatedAt = product.UpdatedAt,
        PendingEvents = product.DomainEvents.Select(DocumentOutboxSerializer.ToPending).ToList()
    };

    public Product ToAggregate() => Product.Restore(
        Id,
        new ProductTitle(Title),
        new Money(Price),
        Description,
        new Category(Category),
        new ImageUrl(Image),
        new Rating(Rate, RatingCount),
        CreatedAt,
        UpdatedAt);
}
