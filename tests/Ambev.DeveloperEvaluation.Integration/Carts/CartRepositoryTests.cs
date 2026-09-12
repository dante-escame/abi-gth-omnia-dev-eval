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

    [Fact]
    public async Task A_Cart_With_Several_Lines_Survives_A_Round_Trip()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var first = new CartItem(new ProductRef(Guid.NewGuid(), "Mountain Bike"), new Quantity(2));
        var second = new CartItem(new ProductRef(Guid.NewGuid(), "Helmet"), new Quantity(7));
        var titleless = new CartItem(new ProductRef(Guid.NewGuid()), new Quantity(1));

        var cart = Cart.Create(customerId, [first, second, titleless]);

        using var scope = factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ICartRepository>();

        // Act
        await repository.CreateAsync(cart);
        var stored = await repository.GetByIdAsync(cart.Id);

        // Assert
        stored.Should().NotBeNull();
        stored!.Id.Should().Be(cart.Id);
        stored.CustomerId.Should().Be(customerId);
        stored.Status.Should().Be(cart.Status);
        stored.CreatedAt.Should().BeCloseTo(cart.CreatedAt, TimeSpan.FromMilliseconds(2));
        stored.Items.Should().BeEquivalentTo(new[] { first, second, titleless });
        stored.Items.Single(item => item.Product.Id == titleless.Product.Id).Product.Title.Should().BeNull();
    }

    [Fact]
    public async Task The_Stored_Document_Uses_The_Documented_Element_Names()
    {
        // Arrange
        var cart = Cart.Create(Guid.NewGuid(), [new CartItem(new ProductRef(Guid.NewGuid(), "Helmet"), new Quantity(3))]);

        using var scope = factory.Services.CreateScope();

        // Act
        await scope.ServiceProvider.GetRequiredService<ICartRepository>().CreateAsync(cart);

        var document = await factory.RawCollection("carts")
            .Find(Builders<BsonDocument>.Filter.Eq("_id", cart.Id))
            .SingleAsync();

        // Assert
        document.Names.Should().Contain(["_id", "userId", "status", "createdAt", "updatedAt", "products"]);

        var line = document["products"].AsBsonArray.Single().AsBsonDocument;

        line.Names.Should().BeEquivalentTo(["productId", "title", "quantity"]);
        line["quantity"].AsInt32.Should().Be(3);
        line["title"].AsString.Should().Be("Helmet");
    }

    [Fact]
    public async Task Updating_A_Cart_Replaces_The_Whole_Embedded_Line_Array()
    {
        // Arrange
        var cart = Cart.Create(Guid.NewGuid(), [new CartItem(new ProductRef(Guid.NewGuid(), "Helmet"), new Quantity(3))]);
        var replacement = new CartItem(new ProductRef(Guid.NewGuid(), "Mountain Bike"), new Quantity(1));

        using var scope = factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ICartRepository>();
        await repository.CreateAsync(cart);

        // Act
        cart.ReplaceItems([replacement]);
        await repository.UpdateAsync(cart);

        // Assert
        var stored = await repository.GetByIdAsync(cart.Id);

        stored!.Items.Should().ContainSingle().Which.Should().Be(replacement);
        stored.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Deleting_A_Cart_Reports_Whether_A_Document_Actually_Matched()
    {
        // Arrange
        var cart = Cart.Create(Guid.NewGuid(), [new CartItem(new ProductRef(Guid.NewGuid()), new Quantity(1))]);

        using var scope = factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ICartRepository>();
        await repository.CreateAsync(cart);

        // Act
        bool firstDelete = await repository.DeleteAsync(cart.Id);
        bool secondDelete = await repository.DeleteAsync(cart.Id);
        bool unknownDelete = await repository.DeleteAsync(Guid.NewGuid());

        // Assert
        firstDelete.Should().BeTrue();
        secondDelete.Should().BeFalse();
        unknownDelete.Should().BeFalse();
        (await repository.GetByIdAsync(cart.Id)).Should().BeNull();
    }
}
