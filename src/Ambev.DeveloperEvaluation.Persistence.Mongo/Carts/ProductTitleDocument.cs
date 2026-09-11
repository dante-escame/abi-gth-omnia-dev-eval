using MongoDB.Bson.Serialization.Attributes;

namespace Ambev.DeveloperEvaluation.Persistence.Mongo.Carts;

public sealed class ProductTitleDocument
{
    [BsonId]
    public Guid ProductId { get; set; }

    [BsonElement("title")]
    public string? Title { get; set; }

    [BsonElement("updatedAtUtc")]
    public DateTime UpdatedAtUtc { get; set; }
}
