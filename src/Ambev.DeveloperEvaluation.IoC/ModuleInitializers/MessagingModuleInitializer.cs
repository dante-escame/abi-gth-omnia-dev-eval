using Ambev.DeveloperEvaluation.Application.Carts.Replication;
using Ambev.DeveloperEvaluation.Catalog.IntegrationEvents;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Config;
using Rebus.Routing.TypeBased;
using Rebus.ServiceProvider;
using Rebus.Transport.InMem;

namespace Ambev.DeveloperEvaluation.IoC.ModuleInitializers;

public class MessagingModuleInitializer : IModuleInitializer
{
    private const string InputQueue = "developer-evaluation";

    public void Initialize(WebApplicationBuilder builder)
    {
        var network = new InMemNetwork();

        builder.Services.AddRebus(
            configure => configure
                .Logging(logging => logging.None())
                .Transport(transport => transport.UseInMemoryTransport(network, InputQueue))
                .Routing(routing => routing.TypeBased().MapAssemblyOf<ProductCreatedIntegrationEvent>(InputQueue)),
            onCreated: async bus =>
            {
                await bus.Subscribe<ProductCreatedIntegrationEvent>();
                await bus.Subscribe<ProductUpdatedIntegrationEvent>();
                await bus.Subscribe<ProductDeletedIntegrationEvent>();
            });

        builder.Services.AutoRegisterHandlersFromAssemblyOf<ProductCatalogEventHandler>();
    }
}
