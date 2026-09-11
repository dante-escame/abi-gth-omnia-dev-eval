using Ambev.DeveloperEvaluation.Catalog.IntegrationEvents;
using MediatR;
using Rebus.Handlers;

namespace Ambev.DeveloperEvaluation.Application.Carts.Replication;

public sealed class ProductCatalogEventHandler(ISender sender) :
    IHandleMessages<ProductCreatedIntegrationEvent>,
    IHandleMessages<ProductUpdatedIntegrationEvent>,
    IHandleMessages<ProductDeletedIntegrationEvent>
{
    public Task Handle(ProductCreatedIntegrationEvent message) =>
        sender.Send(new UpsertProductTitleCommand(message.ProductId, message.Title, message.OccurredOnUtc));

    public Task Handle(ProductUpdatedIntegrationEvent message) =>
        sender.Send(new UpsertProductTitleCommand(message.ProductId, message.Title, message.OccurredOnUtc));

    public Task Handle(ProductDeletedIntegrationEvent message) =>
        sender.Send(new RemoveProductTitleCommand(message.ProductId, message.OccurredOnUtc));
}
