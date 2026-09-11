using Ambev.DeveloperEvaluation.Domain.Carts;
using Ambev.DeveloperEvaluation.Domain.Carts.ValueObjects;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Integration.Common;
using Ambev.DeveloperEvaluation.ORM;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Carts;

[Collection(SalesContextCollection.Name)]
public class CartRepositoryTests(SalesContextApiFactory factory) : IAsyncLifetime
{
    public Task InitializeAsync() => factory.ResetAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [ContainerFact]
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

    [ContainerFact]
    public async Task The_Stored_Rows_Use_The_Documented_Column_Names()
    {
        // Arrange
        var cart = Cart.Create(Guid.NewGuid(), [new CartItem(new ProductRef(Guid.NewGuid(), "Helmet"), new Quantity(3))]);

        using (var writeScope = factory.Services.CreateScope())
        {
            await writeScope.ServiceProvider.GetRequiredService<ICartRepository>().CreateAsync(cart);
        }

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();

        // Act
        var cartColumns = await Columns(context, "carts");
        var lineColumns = await Columns(context, "cart_items");

        // Assert
        cartColumns.Should().Contain(["user_id", "status", "created_at", "updated_at"]);
        lineColumns.Should().Contain(["cart_id", "product_id", "product_title", "quantity"]);
    }

    private static Task<List<string>> Columns(DefaultContext context, string table) =>
        context.Database
            .SqlQueryRaw<string>(
                "select column_name as \"Value\" from information_schema.columns where table_name = {0}",
                table)
            .ToListAsync();

    [ContainerFact]
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

    [ContainerFact]
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
