using System.Text.Json;
using System.Text.Json.Serialization;
using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Persistence.Mongo.Outbox;

public static class DocumentOutboxSerializer
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    public static PendingEventDocument ToPending(IDomainEvent domainEvent) => new()
    {
        Id = domainEvent.Id,
        Type = domainEvent.GetType().FullName!,
        Payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), Options),
        OccurredOnUtc = domainEvent.OccurredOnUtc
    };

    public static IDomainEvent ToDomainEvent(PendingEventDocument pending)
    {
        var type = typeof(IDomainEvent).Assembly.GetType(pending.Type)
            ?? throw new InvalidOperationException($"Pending event type {pending.Type} could not be resolved.");

        return (IDomainEvent)JsonSerializer.Deserialize(pending.Payload, type, Options)!;
    }
}
