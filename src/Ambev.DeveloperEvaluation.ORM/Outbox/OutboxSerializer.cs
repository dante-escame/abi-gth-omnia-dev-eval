using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ambev.DeveloperEvaluation.ORM.Outbox;

public static class OutboxSerializer
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };
}
