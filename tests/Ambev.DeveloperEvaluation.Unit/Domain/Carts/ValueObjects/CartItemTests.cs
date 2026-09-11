using Ambev.DeveloperEvaluation.Domain.Carts.ValueObjects;
using Ambev.DeveloperEvaluation.Unit.Domain.Carts.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Carts.ValueObjects;

public class CartItemTests
{
    [Fact]
    public void Two_Cart_Items_With_The_Same_Product_And_Quantity_Are_Equal()
    {
        // Arrange
        var product = CartTestData.ProductRef();

        // Act
        var left = new CartItem(product, new Quantity(3));
        var right = new CartItem(new ProductRef(product.Id, product.Title), new Quantity(3));

        // Assert
        left.Should().Be(right);
        left.GetHashCode().Should().Be(right.GetHashCode());
    }

    [Fact]
    public void Two_Cart_Items_Differing_In_Quantity_Are_Not_Equal()
    {
        // Arrange
        var product = CartTestData.ProductRef();

        // Act
        var left = new CartItem(product, new Quantity(3));

        // Assert
        left.Should().NotBe(new CartItem(product, new Quantity(4)));
    }

    [Fact]
    public void Replacing_The_Quantity_Leaves_The_Original_Item_Untouched()
    {
        // Arrange
        var item = CartTestData.Item(quantity: 2);

        // Act
        var replaced = item.WithQuantity(new Quantity(9));

        // Assert
        replaced.Should().NotBeSameAs(item);
        replaced.Product.Should().Be(item.Product);
        replaced.Quantity.Value.Should().Be(9);
        item.Quantity.Value.Should().Be(2);
    }

    [Fact]
    public void Constructing_A_Cart_Item_Without_A_Product_Reference_Throws()
    {
        // Act
        var act = () => new CartItem(null!, new Quantity(1));

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructing_A_Cart_Item_Without_A_Quantity_Throws()
    {
        // Act
        var act = () => new CartItem(CartTestData.ProductRef(), null!);

        // Assert
        act.Should().Throw<DomainException>();
    }
}
