using Ambev.DeveloperEvaluation.Domain.Carts;
using Ambev.DeveloperEvaluation.Domain.Carts.ValueObjects;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Unit.Domain.Carts.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Carts;

public class CartTests
{
    [Fact]
    public void Creating_A_Cart_Produces_An_Active_Aggregate_Owned_By_The_Customer()
    {
        // Arrange
        var customerId = CartTestData.CustomerId();
        var item = CartTestData.Item();

        // Act
        var cart = Cart.Create(customerId, [item]);

        // Assert
        cart.Id.Should().NotBeEmpty();
        cart.CustomerId.Should().Be(customerId);
        cart.Status.Should().Be(CartStatus.Active);
        cart.Items.Should().ContainSingle().Which.Should().Be(item);
        cart.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        cart.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void Creating_A_Cart_Raises_No_Domain_Event()
    {
        // Act
        var cart = CartTestData.Cart();

        // Assert
        cart.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Creating_A_Cart_Merges_Duplicate_Lines_Summing_The_Quantities()
    {
        // Arrange
        var productId = CartTestData.ProductId();
        var other = CartTestData.Item(quantity: 1);

        // Act
        var cart = Cart.Create(CartTestData.CustomerId(),
        [
            CartTestData.Item(productId, "Mountain Bike", 2),
            other,
            CartTestData.Item(productId, "Bicicleta", 5)
        ]);

        // Assert
        cart.Items.Should().HaveCount(2);

        var merged = cart.Items.Single(item => item.Product.Id == productId);
        merged.Quantity.Value.Should().Be(7);
        merged.Product.Title.Should().Be("Mountain Bike");

        cart.Items.Last().Should().Be(other);
    }

    [Fact]
    public void Creating_A_Cart_Without_A_Customer_Throws()
    {
        // Act
        var act = () => Cart.Create(Guid.Empty, [CartTestData.Item()]);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Creating_A_Cart_Without_A_Line_List_Throws()
    {
        // Act
        var act = () => Cart.Create(CartTestData.CustomerId(), null!);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Creating_A_Cart_With_An_Empty_Line_Throws()
    {
        // Act
        var act = () => Cart.Create(CartTestData.CustomerId(), [null!]);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Replacing_The_Lines_Swaps_The_Whole_Set_And_Stamps_The_Update_Time()
    {
        // Arrange
        var cart = CartTestData.Cart(CartTestData.Item(quantity: 3));
        var replacement = CartTestData.Item(quantity: 8);

        // Act
        cart.ReplaceItems([replacement]);

        // Assert
        cart.Items.Should().ContainSingle().Which.Should().Be(replacement);
        cart.UpdatedAt.Should().NotBeNull().And.BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Replacing_The_Lines_Merges_Duplicates_The_Same_Way_Creation_Does()
    {
        // Arrange
        var productId = CartTestData.ProductId();
        var cart = CartTestData.Cart();

        // Act
        cart.ReplaceItems(
        [
            CartTestData.Item(productId, "Helmet", 4),
            CartTestData.Item(productId, "Capacete", 6)
        ]);

        // Assert
        var merged = cart.Items.Should().ContainSingle().Subject;
        merged.Quantity.Value.Should().Be(10);
        merged.Product.Title.Should().Be("Helmet");
    }

    [Theory]
    [InlineData(CartStatus.CheckedOut)]
    [InlineData(CartStatus.Abandoned)]
    public void A_Cart_That_Is_No_Longer_Active_Refuses_Every_Mutation(CartStatus status)
    {
        // Arrange
        var cart = CartTestData.RestoredCart(status);

        // Act
        var act = () => cart.ReplaceItems([CartTestData.Item()]);

        // Assert
        act.Should().Throw<DomainException>();
        cart.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void Restoring_A_Cart_Keeps_Its_Identity_Status_And_Timestamps()
    {
        // Arrange
        var id = Guid.NewGuid();
        var customerId = CartTestData.CustomerId();
        var createdAt = new DateTime(2026, 3, 4, 5, 6, 7, DateTimeKind.Utc);
        var updatedAt = createdAt.AddHours(2);

        // Act
        var cart = Cart.Restore(id, customerId, CartStatus.CheckedOut, [CartTestData.Item()], createdAt, updatedAt);

        // Assert
        cart.Id.Should().Be(id);
        cart.CustomerId.Should().Be(customerId);
        cart.Status.Should().Be(CartStatus.CheckedOut);
        cart.CreatedAt.Should().Be(createdAt);
        cart.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public void The_Line_Set_Cannot_Be_Changed_From_Outside_The_Aggregate()
    {
        // Arrange
        var cart = CartTestData.Cart();

        // Act
        var act = () => ((IList<CartItem>)cart.Items).Add(CartTestData.Item());

        // Assert
        act.Should().Throw<NotSupportedException>();
    }
}
