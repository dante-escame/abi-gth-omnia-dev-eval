using Ambev.DeveloperEvaluation.Domain.Carts;
using Ambev.DeveloperEvaluation.Domain.Carts.ValueObjects;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Unit.Domain.Carts.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Carts;

public class CartTests
{
    [Fact(DisplayName = "Creating a cart produces an active aggregate owned by the customer")]
    public void Given_ValidLines_When_Created_Then_IsActive()
    {
        var customerId = CartTestData.CustomerId();
        var item = CartTestData.Item();

        var cart = Cart.Create(customerId, [item]);

        cart.Id.Should().NotBeEmpty();
        cart.CustomerId.Should().Be(customerId);
        cart.Status.Should().Be(CartStatus.Active);
        cart.Items.Should().ContainSingle().Which.Should().Be(item);
        cart.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        cart.UpdatedAt.Should().BeNull();
    }

    [Fact(DisplayName = "Creating a cart raises no domain event")]
    public void Given_NewCart_When_Created_Then_RaisesNothing()
    {
        CartTestData.Cart().DomainEvents.Should().BeEmpty();
    }

    [Fact(DisplayName = "Creating a cart merges duplicate lines into one summed line")]
    public void Given_DuplicateLines_When_Created_Then_MergesAndSums()
    {
        var productId = CartTestData.ProductId();
        var other = CartTestData.Item(quantity: 1);

        var cart = Cart.Create(CartTestData.CustomerId(),
        [
            CartTestData.Item(productId, "Mountain Bike", 2),
            other,
            CartTestData.Item(productId, "Bicicleta", 5)
        ]);

        cart.Items.Should().HaveCount(2);

        var merged = cart.Items.Single(item => item.Product.Id == productId);
        merged.Quantity.Value.Should().Be(7);
        merged.Product.Title.Should().Be("Mountain Bike");

        cart.Items.Last().Should().Be(other);
    }

    [Fact(DisplayName = "Creating a cart without a customer throws")]
    public void Given_NoCustomer_When_Created_Then_Throws()
    {
        var act = () => Cart.Create(Guid.Empty, [CartTestData.Item()]);

        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "Creating a cart without a line list throws")]
    public void Given_NoLineList_When_Created_Then_Throws()
    {
        var act = () => Cart.Create(CartTestData.CustomerId(), null!);

        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "Creating a cart with an empty line throws")]
    public void Given_EmptyLine_When_Created_Then_Throws()
    {
        var act = () => Cart.Create(CartTestData.CustomerId(), [null!]);

        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "Replacing the lines swaps the whole set and stamps the update time")]
    public void Given_ActiveCart_When_ItemsReplaced_Then_SwapsAndTouches()
    {
        var cart = CartTestData.Cart(CartTestData.Item(quantity: 3));
        var replacement = CartTestData.Item(quantity: 8);

        cart.ReplaceItems([replacement]);

        cart.Items.Should().ContainSingle().Which.Should().Be(replacement);
        cart.UpdatedAt.Should().NotBeNull().And.BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact(DisplayName = "Replacing the lines merges duplicates the same way creation does")]
    public void Given_DuplicateLines_When_ItemsReplaced_Then_MergesAndSums()
    {
        var productId = CartTestData.ProductId();
        var cart = CartTestData.Cart();

        cart.ReplaceItems(
        [
            CartTestData.Item(productId, "Helmet", 4),
            CartTestData.Item(productId, "Capacete", 6)
        ]);

        var merged = cart.Items.Should().ContainSingle().Subject;
        merged.Quantity.Value.Should().Be(10);
        merged.Product.Title.Should().Be("Helmet");
    }

    [Theory(DisplayName = "A cart that is no longer active refuses every mutation")]
    [InlineData(CartStatus.CheckedOut)]
    [InlineData(CartStatus.Abandoned)]
    public void Given_InactiveCart_When_Mutated_Then_Throws(CartStatus status)
    {
        var cart = CartTestData.RestoredCart(status);

        var act = () => cart.ReplaceItems([CartTestData.Item()]);

        act.Should().Throw<DomainException>();
        cart.UpdatedAt.Should().BeNull();
    }

    [Fact(DisplayName = "Restoring a cart keeps its identity, status and timestamps")]
    public void Given_StoredValues_When_Restored_Then_KeepsState()
    {
        var id = Guid.NewGuid();
        var customerId = CartTestData.CustomerId();
        var createdAt = new DateTime(2026, 3, 4, 5, 6, 7, DateTimeKind.Utc);
        var updatedAt = createdAt.AddHours(2);

        var cart = Cart.Restore(id, customerId, CartStatus.CheckedOut, [CartTestData.Item()], createdAt, updatedAt);

        cart.Id.Should().Be(id);
        cart.CustomerId.Should().Be(customerId);
        cart.Status.Should().Be(CartStatus.CheckedOut);
        cart.CreatedAt.Should().Be(createdAt);
        cart.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact(DisplayName = "The line set cannot be changed from outside the aggregate")]
    public void Given_Cart_When_ItemsMutatedDirectly_Then_Throws()
    {
        var cart = CartTestData.Cart();

        var act = () => ((IList<CartItem>)cart.Items).Add(CartTestData.Item());

        act.Should().Throw<NotSupportedException>();
    }
}
