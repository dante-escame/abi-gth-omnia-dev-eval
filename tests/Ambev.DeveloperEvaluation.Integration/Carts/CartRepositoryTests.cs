using Ambev.DeveloperEvaluation.Domain.Carts;
using Ambev.DeveloperEvaluation.Domain.Carts.ValueObjects;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Carts;

[Collection(CartsCollection.Name)]
public class CartRepositoryTests(CartsApiFactory factory) : IAsyncLifetime
{
    public Task InitializeAsync() => factory.ResetAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact(DisplayName = "A cart with several lines survives a round trip")]
    public async Task Given_ManyLines_When_Stored_Then_ReadsBackIdentical()
    {
        var customerId = Guid.NewGuid();
        var first = new CartItem(new ProductRef(Guid.NewGuid(), "Mountain Bike"), new Quantity(2));
        var second = new CartItem(new ProductRef(Guid.NewGuid(), "Helmet"), new Quantity(7));
        var titleless = new CartItem(new ProductRef(Guid.NewGuid()), new Quantity(1));

        var cart = Cart.Create(customerId, [first, second, titleless]);

        using var scope = factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ICartRepository>();

        await repository.CreateAsync(cart);

        var stored = await repository.GetByIdAsync(cart.Id);

        stored.Should().NotBeNull();
        stored!.Id.Should().Be(cart.Id);
        stored.CustomerId.Should().Be(customerId);
        stored.Status.Should().Be(cart.Status);
        stored.CreatedAt.Should().BeCloseTo(cart.CreatedAt, TimeSpan.FromMilliseconds(1));
        stored.Items.Should().BeEquivalentTo(new[] { first, second, titleless });
        stored.Items.Single(item => item.Product.Id == titleless.Product.Id).Product.Title.Should().BeNull();
    }

    [Fact(DisplayName = "The stored document uses the documented element names")]
    public async Task Given_Cart_When_Stored_Then_UsesContractNames()
    {
        var cart = Cart.Create(Guid.NewGuid(), [new CartItem(new ProductRef(Guid.NewGuid(), "Helmet"), new Quantity(3))]);

        using var scope = factory.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<ICartRepository>().CreateAsync(cart);

        var document = await factory.RawCollection("carts")
            .Find(Builders<BsonDocument>.Filter.Eq("_id", cart.Id))
            .SingleAsync();

        document.Names.Should().Contain(["_id", "userId", "status", "createdAt", "updatedAt", "products"]);

        var line = document["products"].AsBsonArray.Single().AsBsonDocument;

        line.Names.Should().BeEquivalentTo(["productId", "title", "quantity"]);
        line["quantity"].AsInt32.Should().Be(3);
        line["title"].AsString.Should().Be("Helmet");
    }

    [Fact(DisplayName = "Updating a cart replaces the whole embedded line array")]
    public async Task Given_StoredCart_When_Updated_Then_ReplacesLines()
    {
        var cart = Cart.Create(Guid.NewGuid(), [new CartItem(new ProductRef(Guid.NewGuid(), "Helmet"), new Quantity(3))]);
        var replacement = new CartItem(new ProductRef(Guid.NewGuid(), "Mountain Bike"), new Quantity(1));

        using var scope = factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ICartRepository>();

        await repository.CreateAsync(cart);
        cart.ReplaceItems([replacement]);
        await repository.UpdateAsync(cart);

        var stored = await repository.GetByIdAsync(cart.Id);

        stored!.Items.Should().ContainSingle().Which.Should().Be(replacement);
        stored.UpdatedAt.Should().NotBeNull();
    }

    [Fact(DisplayName = "Deleting reports whether a document actually matched")]
    public async Task Given_UnknownId_When_Deleted_Then_ReturnsFalse()
    {
        var cart = Cart.Create(Guid.NewGuid(), [new CartItem(new ProductRef(Guid.NewGuid()), new Quantity(1))]);

        using var scope = factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ICartRepository>();

        await repository.CreateAsync(cart);

        (await repository.DeleteAsync(cart.Id)).Should().BeTrue();
        (await repository.DeleteAsync(cart.Id)).Should().BeFalse();
        (await repository.DeleteAsync(Guid.NewGuid())).Should().BeFalse();
        (await repository.GetByIdAsync(cart.Id)).Should().BeNull();
    }
}
