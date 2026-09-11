using Ambev.DeveloperEvaluation.Application.Carts.Replication;
using Ambev.DeveloperEvaluation.Catalog.IntegrationEvents;
using Ambev.DeveloperEvaluation.Unit.Application.Carts.TestData;
using FluentAssertions;
using MediatR;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Carts;

public class ProductCatalogEventHandlerTests
{
    private readonly ISender _sender = Substitute.For<ISender>();

    private readonly ProductCatalogEventHandler _handler;

    public ProductCatalogEventHandlerTests() => _handler = new ProductCatalogEventHandler(_sender);

    [Fact(DisplayName = "A created product becomes an upsert carrying the event time")]
    public async Task Given_Created_When_Handled_Then_SendsUpsert()
    {
        var productId = CatalogEventTestData.ProductId();
        string title = CatalogEventTestData.Title();
        var occurredOn = CatalogEventTestData.OccurredOn();

        await _handler.Handle(new ProductCreatedIntegrationEvent(productId, title, occurredOn));

        await _sender.Received(1).Send(
            Arg.Is<UpsertProductTitleCommand>(command =>
                command.ProductId == productId &&
                command.Title == title &&
                command.OccurredOnUtc == occurredOn),
            Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "An updated product becomes an upsert carrying the event time")]
    public async Task Given_Updated_When_Handled_Then_SendsUpsert()
    {
        var productId = CatalogEventTestData.ProductId();
        string title = CatalogEventTestData.Title();
        var occurredOn = CatalogEventTestData.OccurredOn();

        await _handler.Handle(new ProductUpdatedIntegrationEvent(productId, title, occurredOn));

        await _sender.Received(1).Send(
            Arg.Is<UpsertProductTitleCommand>(command =>
                command.ProductId == productId &&
                command.Title == title &&
                command.OccurredOnUtc == occurredOn),
            Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "A deleted product becomes a removal carrying the event time")]
    public async Task Given_Deleted_When_Handled_Then_SendsRemoval()
    {
        var productId = CatalogEventTestData.ProductId();
        var occurredOn = CatalogEventTestData.OccurredOn();

        await _handler.Handle(new ProductDeletedIntegrationEvent(productId, occurredOn));

        await _sender.Received(1).Send(
            Arg.Is<RemoveProductTitleCommand>(command =>
                command.ProductId == productId &&
                command.OccurredOnUtc == occurredOn),
            Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "The bus handler does nothing but send a command")]
    public async Task Given_AnyEvent_When_Handled_Then_TouchesNothingElse()
    {
        await _handler.Handle(new ProductCreatedIntegrationEvent(
            CatalogEventTestData.ProductId(),
            CatalogEventTestData.Title(),
            CatalogEventTestData.OccurredOn()));

        _sender.ReceivedCalls().Should().ContainSingle();
    }
}
