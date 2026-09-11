using Ambev.DeveloperEvaluation.Domain.Carts;
using Ambev.DeveloperEvaluation.Domain.Carts.ValueObjects;
using Ambev.DeveloperEvaluation.Domain.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace Ambev.DeveloperEvaluation.Persistence.Mongo.Carts;

public sealed class CartDocument
{
    [BsonId]
    public Guid Id { get; set; }

    [BsonElement("userId")]
    public Guid UserId { get; set; }

    [BsonElement("status")]
    public string Status { get; set; } = nameof(CartStatus.Active);

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    public DateTime? UpdatedAt { get; set; }

    [BsonElement("products")]
    public List<CartLineDocument> Products { get; set; } = [];

    public static CartDocument From(Cart cart) => new()
    {
        Id = cart.Id,
        UserId = cart.CustomerId,
        Status = cart.Status.ToString(),
        CreatedAt = cart.CreatedAt,
        UpdatedAt = cart.UpdatedAt,
        Products = cart.Items.Select(CartLineDocument.From).ToList()
    };

    public Cart ToAggregate() => Cart.Restore(
        Id,
        UserId,
        Enum.Parse<CartStatus>(Status, ignoreCase: true),
        Products.Select(line => line.ToItem()),
        CreatedAt,
        UpdatedAt);
}

public sealed class CartLineDocument
{
    [BsonElement("productId")]
    public Guid ProductId { get; set; }

    [BsonElement("title")]
    public string? Title { get; set; }

    [BsonElement("quantity")]
    public int Quantity { get; set; }

    public static CartLineDocument From(CartItem item) => new()
    {
        ProductId = item.Product.Id,
        Title = item.Product.Title,
        Quantity = item.Quantity.Value
    };

    public CartItem ToItem() => new(new ProductRef(ProductId, Title), new Quantity(Quantity));
}
