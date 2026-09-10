using System.Text.Json.Serialization;

namespace Ambev.DeveloperEvaluation.WebApi.Common;

public sealed record ErrorResponse(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("error")] string Error,
    [property: JsonPropertyName("detail")] string Detail);
