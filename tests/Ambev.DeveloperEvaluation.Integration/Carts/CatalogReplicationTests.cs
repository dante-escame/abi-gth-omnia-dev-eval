using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ambev.DeveloperEvaluation.Application.Ports;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Products.Events;
using Ambev.DeveloperEvaluation.Persistence.Mongo.Products;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Carts;

[Collection(CartsCollection.Name)]
public class CatalogReplicationTests(CartsApiFactory factory) : IAsyncLifetime
{
    private static readonly TimeSpan Patience = TimeSpan.FromSeconds(10);

    public Task InitializeAsync() => factory.ResetAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact(DisplayName = "Creating a product raises an event inside the document until a catalog cycle happens")]
    public async Task Given_CreatedProduct_When_CycleRuns_Then_DrainsPendingEvents()
    {
        var (manager, _) = await factory.SignInAsync(UserRole.Manager);

        var productId = await manager.CreateProductAsync("Mountain Bike");

        var pending = await ProductAsync(productId);
        pending.PendingEvents.Should().ContainSingle()
            .Which.Type.Should().EndWith(nameof(ProductCreatedDomainEvent));

        (await factory.RunOutboxCycleAsync()).Should().Be(1);

        (await ProductAsync(productId)).PendingEvents.Should().BeEmpty();
    }

    [Fact(DisplayName = "A rename reaches the cart line through the bus")]
    public async Task Given_RenamedProduct_When_CartWritten_Then_CarriesTheNewTitle()
    {
        var (manager, _) = await factory.SignInAsync(UserRole.Manager);
        var (customer, _) = await factory.SignInAsync(UserRole.Customer);

        var productId = await manager.CreateProductAsync("Mountain Bike");
        await factory.RunOutboxCycleAsync();
        await WaitForTitleAsync(productId, "Mountain Bike");

        var created = await customer.CreateCartAsync((productId, 2));
        TitleOf(created, productId).Should().Be("Mountain Bike");

        await manager.RenameProductAsync(productId, "Racing Bike");
        await factory.RunOutboxCycleAsync();
        await WaitForTitleAsync(productId, "Racing Bike");

        var response = await customer.PutAsJsonAsync(
            $"/api/carts/{created.GetProperty("id").GetString()}",
            CartsTestClient.CartBody((productId, 3)));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var updated = await response.Content.ReadFromJsonAsync<JsonElement>();
        TitleOf(updated, productId).Should().Be("Racing Bike");
    }

    [Fact(DisplayName = "A delete keeps a tombstone until its event is published, then clears the replica")]
    public async Task Given_DeletedProduct_When_CycleRuns_Then_RemovesDocumentAndTitle()
    {
        var (manager, _) = await factory.SignInAsync(UserRole.Manager);

        var productId = await manager.CreateProductAsync("Helmet");
        await factory.RunOutboxCycleAsync();
        await WaitForTitleAsync(productId, "Helmet");

        (await manager.DeleteAsync($"/api/products/{productId}")).StatusCode
            .Should().Be(HttpStatusCode.NoContent);

        var tombstone = await ProductAsync(productId);
        tombstone.DeletedAt.Should().NotBeNull();
        tombstone.PendingEvents.Should().ContainSingle()
            .Which.Type.Should().EndWith(nameof(ProductDeletedDomainEvent));

        await factory.RunOutboxCycleAsync();

        (await factory.Catalog.Products.Find(document => document.Id == productId).AnyAsync())
            .Should().BeFalse();

        await WaitForTitleAsync(productId, null);
    }

    [Fact(DisplayName = "A product the replica never heard of yields a line with no title")]
    public async Task Given_UnknownProduct_When_CartCreated_Then_TitleIsNull()
    {
        var (customer, _) = await factory.SignInAsync(UserRole.Customer);
        var productId = Guid.NewGuid();

        var created = await customer.CreateCartAsync((productId, 1));

        created.GetProperty("products").EnumerateArray().Single()
            .GetProperty("title").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact(DisplayName = "The replica keeps the newest snapshot whatever order the events arrive in")]
    public async Task Given_OutOfOrderEvents_When_Applied_Then_KeepsTheNewest()
    {
        var productId = Guid.NewGuid();
        var occurredOn = DateTime.UtcNow;

        using var scope = factory.Services.CreateScope();
        var replica = scope.ServiceProvider.GetRequiredService<IProductTitles>();

        await replica.UpsertAsync(productId, "Mountain Bike", occurredOn);
        (await TitleAsync(replica, productId)).Should().Be("Mountain Bike");

        await replica.UpsertAsync(productId, "Racing Bike", occurredOn.AddMinutes(5));
        (await TitleAsync(replica, productId)).Should().Be("Racing Bike");

        await replica.UpsertAsync(productId, "Mountain Bike", occurredOn.AddMinutes(1));
        (await TitleAsync(replica, productId)).Should().Be("Racing Bike");

        await replica.UpsertAsync(productId, "Racing Bike", occurredOn.AddMinutes(5));
        (await TitleAsync(replica, productId)).Should().Be("Racing Bike");

        await replica.RemoveAsync(productId, occurredOn.AddMinutes(2));
        (await TitleAsync(replica, productId)).Should().Be("Racing Bike");

        await replica.RemoveAsync(productId, occurredOn.AddMinutes(9));
        (await TitleAsync(replica, productId)).Should().BeNull();
    }

    private static async Task<string?> TitleAsync(IProductTitles replica, Guid productId) =>
        (await replica.ResolveAsync([productId])).GetValueOrDefault(productId);

    private static string? TitleOf(JsonElement cart, Guid productId) => cart
        .GetProperty("products")
        .EnumerateArray()
        .Single(line => Guid.Parse(line.GetProperty("productId").GetString()!) == productId)
        .GetProperty("title")
        .GetString();

    private Task<ProductDocument> ProductAsync(Guid productId) =>
        factory.Catalog.Products.Find(document => document.Id == productId).SingleAsync();

    private async Task WaitForTitleAsync(Guid productId, string? expected)
    {
        var deadline = DateTime.UtcNow + Patience;

        while (DateTime.UtcNow < deadline)
        {
            var document = await factory.Carts.ProductTitles
                .Find(entry => entry.ProductId == productId)
                .FirstOrDefaultAsync();

            if (document?.Title == expected)
                return;

            await Task.Delay(50);
        }

        throw new Xunit.Sdk.XunitException(
            $"The replica never reached the title {expected ?? "<none>"} for {productId}");
    }
}
