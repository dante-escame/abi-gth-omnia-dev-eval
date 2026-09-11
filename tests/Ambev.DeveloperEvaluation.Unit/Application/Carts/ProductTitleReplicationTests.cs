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

    [Fact(DisplayName = "A newer event overwrites the stored title")]
    public async Task Given_NewerEvent_When_Handled_Then_Overwrites()
    {
        var productId = CatalogEventTestData.ProductId();
        var first = CatalogEventTestData.OccurredOn();

        await UpsertAsync(productId, "Mountain Bike", first);
        await UpsertAsync(productId, "Racing Bike", first.AddMinutes(5));

        (await _replica.TitleOfAsync(productId)).Should().Be("Racing Bike");
    }

    [Fact(DisplayName = "An event older than the stored snapshot is ignored")]
    public async Task Given_OlderEvent_When_Handled_Then_KeepsStoredTitle()
    {
        var productId = CatalogEventTestData.ProductId();
        var current = CatalogEventTestData.OccurredOn();

        await UpsertAsync(productId, "Racing Bike", current);
        await UpsertAsync(productId, "Mountain Bike", current.AddMinutes(-5));

        (await _replica.TitleOfAsync(productId)).Should().Be("Racing Bike");
    }

    [Fact(DisplayName = "A redelivered event leaves the replica exactly as it was")]
    public async Task Given_RedeliveredEvent_When_Handled_Then_ChangesNothing()
    {
        var productId = CatalogEventTestData.ProductId();
        var occurredOn = CatalogEventTestData.OccurredOn();

        await UpsertAsync(productId, "Mountain Bike", occurredOn);
        await UpsertAsync(productId, "Mountain Bike", occurredOn);
        await UpsertAsync(productId, "Mountain Bike", occurredOn);

        (await _replica.TitleOfAsync(productId)).Should().Be("Mountain Bike");
    }

    [Fact(DisplayName = "A removal clears the title so later cart lines resolve to nothing")]
    public async Task Given_Removal_When_Handled_Then_TitleIsGone()
    {
        var productId = CatalogEventTestData.ProductId();
        var occurredOn = CatalogEventTestData.OccurredOn();

        await UpsertAsync(productId, "Mountain Bike", occurredOn);
        await _remove.Handle(new RemoveProductTitleCommand(productId, occurredOn.AddMinutes(1)), default);

        (await _replica.TitleOfAsync(productId)).Should().BeNull();
    }

    [Fact(DisplayName = "A removal older than the stored snapshot is ignored")]
    public async Task Given_OlderRemoval_When_Handled_Then_KeepsStoredTitle()
    {
        var productId = CatalogEventTestData.ProductId();
        var occurredOn = CatalogEventTestData.OccurredOn();

        await UpsertAsync(productId, "Mountain Bike", occurredOn);
        await _remove.Handle(new RemoveProductTitleCommand(productId, occurredOn.AddMinutes(-1)), default);

        (await _replica.TitleOfAsync(productId)).Should().Be("Mountain Bike");
    }

    [Fact(DisplayName = "The upsert handler forwards the event time untouched")]
    public async Task Given_Command_When_Handled_Then_ForwardsEventTime()
    {
        var titles = Substitute.For<IProductTitles>();
        var productId = CatalogEventTestData.ProductId();
        string title = CatalogEventTestData.Title();
        var occurredOn = CatalogEventTestData.OccurredOn();

        await new UpsertProductTitleHandler(titles)
            .Handle(new UpsertProductTitleCommand(productId, title, occurredOn), default);

        await titles.Received(1).UpsertAsync(productId, title, occurredOn, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "The removal handler forwards the event time untouched")]
    public async Task Given_RemovalCommand_When_Handled_Then_ForwardsEventTime()
    {
        var titles = Substitute.For<IProductTitles>();
        var productId = CatalogEventTestData.ProductId();
        var occurredOn = CatalogEventTestData.OccurredOn();

        await new RemoveProductTitleHandler(titles)
            .Handle(new RemoveProductTitleCommand(productId, occurredOn), default);

        await titles.Received(1).RemoveAsync(productId, occurredOn, Arg.Any<CancellationToken>());
    }

    private Task UpsertAsync(Guid productId, string title, DateTime occurredOn) =>
        _upsert.Handle(new UpsertProductTitleCommand(productId, title, occurredOn), default);
}
