using Ambev.DeveloperEvaluation.Application.Carts.Replication;
using Ambev.DeveloperEvaluation.Application.Ports;
using Ambev.DeveloperEvaluation.Unit.Application.Carts.TestData;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Carts;

public class ProductTitleReplicationTests
{
    private readonly ReplicaDouble _replica = new();

    private readonly UpsertProductTitleHandler _upsert;

    private readonly RemoveProductTitleHandler _remove;

    public ProductTitleReplicationTests()
    {
        _upsert = new UpsertProductTitleHandler(_replica);
        _remove = new RemoveProductTitleHandler(_replica);
    }

    [Fact]
    public async Task An_Event_Newer_Than_The_Stored_Snapshot_Overwrites_The_Title()
    {
        // Arrange
        var productId = CatalogEventTestData.ProductId();
        var first = CatalogEventTestData.OccurredOn();
        await UpsertAsync(productId, "Mountain Bike", first);

        // Act
        await UpsertAsync(productId, "Racing Bike", first.AddMinutes(5));

        // Assert
        (await _replica.TitleOfAsync(productId)).Should().Be("Racing Bike");
    }

    [Fact]
    public async Task An_Event_Older_Than_The_Stored_Snapshot_Is_Ignored()
    {
        // Arrange
        var productId = CatalogEventTestData.ProductId();
        var current = CatalogEventTestData.OccurredOn();
        await UpsertAsync(productId, "Racing Bike", current);

        // Act
        await UpsertAsync(productId, "Mountain Bike", current.AddMinutes(-5));

        // Assert
        (await _replica.TitleOfAsync(productId)).Should().Be("Racing Bike");
    }

    [Fact]
    public async Task A_Redelivered_Event_Leaves_The_Replica_Exactly_As_It_Was()
    {
        // Arrange
        var productId = CatalogEventTestData.ProductId();
        var occurredOn = CatalogEventTestData.OccurredOn();
        await UpsertAsync(productId, "Mountain Bike", occurredOn);

        // Act
        await UpsertAsync(productId, "Mountain Bike", occurredOn);
        await UpsertAsync(productId, "Mountain Bike", occurredOn);

        // Assert
        (await _replica.TitleOfAsync(productId)).Should().Be("Mountain Bike");
    }

    [Fact]
    public async Task A_Removal_Clears_The_Title_So_Later_Cart_Lines_Resolve_To_Nothing()
    {
        // Arrange
        var productId = CatalogEventTestData.ProductId();
        var occurredOn = CatalogEventTestData.OccurredOn();
        await UpsertAsync(productId, "Mountain Bike", occurredOn);

        // Act
        await _remove.Handle(new RemoveProductTitleCommand(productId, occurredOn.AddMinutes(1)), default);

        // Assert
        (await _replica.TitleOfAsync(productId)).Should().BeNull();
    }

    [Fact]
    public async Task A_Removal_Older_Than_The_Stored_Snapshot_Is_Ignored()
    {
        // Arrange
        var productId = CatalogEventTestData.ProductId();
        var occurredOn = CatalogEventTestData.OccurredOn();
        await UpsertAsync(productId, "Mountain Bike", occurredOn);

        // Act
        await _remove.Handle(new RemoveProductTitleCommand(productId, occurredOn.AddMinutes(-1)), default);

        // Assert
        (await _replica.TitleOfAsync(productId)).Should().Be("Mountain Bike");
    }

    [Fact]
    public async Task The_Upsert_Handler_Forwards_The_Event_Time_Untouched()
    {
        // Arrange
        var titles = Substitute.For<IProductTitles>();
        var productId = CatalogEventTestData.ProductId();
        string title = CatalogEventTestData.Title();
        var occurredOn = CatalogEventTestData.OccurredOn();

        // Act
        await new UpsertProductTitleHandler(titles)
            .Handle(new UpsertProductTitleCommand(productId, title, occurredOn), default);

        // Assert
        await titles.Received(1).UpsertAsync(productId, title, occurredOn, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task The_Removal_Handler_Forwards_The_Event_Time_Untouched()
    {
        // Arrange
        var titles = Substitute.For<IProductTitles>();
        var productId = CatalogEventTestData.ProductId();
        var occurredOn = CatalogEventTestData.OccurredOn();

        // Act
        await new RemoveProductTitleHandler(titles)
            .Handle(new RemoveProductTitleCommand(productId, occurredOn), default);

        // Assert
        await titles.Received(1).RemoveAsync(productId, occurredOn, Arg.Any<CancellationToken>());
    }

    private Task UpsertAsync(Guid productId, string title, DateTime occurredOn) =>
        _upsert.Handle(new UpsertProductTitleCommand(productId, title, occurredOn), default);
}
