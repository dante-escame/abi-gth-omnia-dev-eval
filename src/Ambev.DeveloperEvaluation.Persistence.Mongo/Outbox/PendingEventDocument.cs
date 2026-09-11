using MongoDB.Bson.Serialization.Attributes;

namespace Ambev.DeveloperEvaluation.Persistence.Mongo.Outbox;

public sealed class PendingEventDocument
{
    [BsonElement("id")]
    public Guid Id { get; set; }

    [BsonElement("type")]
    public string Type { get; set; } = string.Empty;

    [BsonElement("payload")]
    public string Payload { get; set; } = string.Empty;

    [BsonElement("occurredOnUtc")]
    public DateTime OccurredOnUtc { get; set; }
}
