using System.Text.Json;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Ambev.DeveloperEvaluation.Application.Sales.Events;

internal static class SaleEventPublishing
{
    private static readonly JsonSerializerOptions PayloadOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static async Task PublishAsync<TIntegrationEvent>(
        IBus bus,
        ILogger logger,
        TIntegrationEvent integrationEvent)
        where TIntegrationEvent : notnull
    {
        await bus.Publish(integrationEvent);

        logger.LogInformation(
            "Published {IntegrationEvent} {Payload}",
            typeof(TIntegrationEvent).Name,
            JsonSerializer.Serialize(integrationEvent, PayloadOptions));
    }
}
