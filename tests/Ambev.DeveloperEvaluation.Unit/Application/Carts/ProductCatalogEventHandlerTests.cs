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

    [Fact]
    public async Task A_Created_Product_Becomes_An_Upsert_Carrying_The_Event_Time()
    {
        // Arrange
        var productId = CatalogEventTestData.ProductId();
        string title = CatalogEventTestData.Title();
        var occurredOn = CatalogEventTestData.OccurredOn();

        // Act
        await _handler.Handle(new ProductCreatedIntegrationEvent(productId, title, occurredOn));

        // Assert
        await _sender.Received(1).Send(
            Arg.Is<UpsertProductTitleCommand>(command =>
                command.ProductId == productId &&
                command.Title == title &&
                command.OccurredOnUtc == occurredOn),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task An_Updated_Product_Becomes_An_Upsert_Carrying_The_Event_Time()
    {
        // Arrange
        var productId = CatalogEventTestData.ProductId();
        string title = CatalogEventTestData.Title();
        var occurredOn = CatalogEventTestData.OccurredOn();

        // Act
        await _handler.Handle(new ProductUpdatedIntegrationEvent(productId, title, occurredOn));

        // Assert
        await _sender.Received(1).Send(
            Arg.Is<UpsertProductTitleCommand>(command =>
                command.ProductId == productId &&
                command.Title == title &&
                command.OccurredOnUtc == occurredOn),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_Deleted_Product_Becomes_A_Removal_Carrying_The_Event_Time()
    {
        // Arrange
        var productId = CatalogEventTestData.ProductId();
        var occurredOn = CatalogEventTestData.OccurredOn();

        // Act
        await _handler.Handle(new ProductDeletedIntegrationEvent(productId, occurredOn));

        // Assert
        await _sender.Received(1).Send(
            Arg.Is<RemoveProductTitleCommand>(command =>
                command.ProductId == productId &&
                command.OccurredOnUtc == occurredOn),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task The_Bus_Handler_Does_Nothing_But_Send_A_Command()
    {
        // Arrange
        var integrationEvent = new ProductCreatedIntegrationEvent(
            CatalogEventTestData.ProductId(),
            CatalogEventTestData.Title(),
            CatalogEventTestData.OccurredOn());

        // Act
        await _handler.Handle(integrationEvent);

        // Assert
        _sender.ReceivedCalls().Should().ContainSingle();
    }
}
