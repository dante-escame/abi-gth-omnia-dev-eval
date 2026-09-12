using Ambev.DeveloperEvaluation.Catalog.IntegrationEvents;
using Ambev.DeveloperEvaluation.Sales.IntegrationEvents;
using Microsoft.AspNetCore.Builder;
using Rebus.Config;
using Rebus.Routing.TypeBased;
using Rebus.Transport.InMem;

namespace Ambev.DeveloperEvaluation.IoC.ModuleInitializers;

public class MessagingModuleInitializer : IModuleInitializer
{
    private const string InputQueue = "developer-evaluation";

    public void Initialize(WebApplicationBuilder builder)
    {
        var network = new InMemNetwork();

        builder.Services.AddRebus(configure => configure
            .Logging(logging => logging.None())
            .Transport(transport => transport.UseInMemoryTransport(network, InputQueue))
            .Routing(routing => routing.TypeBased()
                .MapAssemblyOf<ProductCreatedIntegrationEvent>(InputQueue)
                .MapAssemblyOf<SaleCreatedIntegrationEvent>(InputQueue)));
    }
}
