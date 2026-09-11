using Ambev.DeveloperEvaluation.Domain.Carts.ValueObjects;
using Ambev.DeveloperEvaluation.Unit.Domain.Carts.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Carts.ValueObjects;

public class CartItemTests
{
    [Fact(DisplayName = "Cart items with the same product and quantity are equal")]
    public void Given_SameComponents_When_Compared_Then_IsEqual()
    {
        var product = CartTestData.ProductRef();

        var left = new CartItem(product, new Quantity(3));
        var right = new CartItem(new ProductRef(product.Id, product.Title), new Quantity(3));

        left.Should().Be(right);
        left.GetHashCode().Should().Be(right.GetHashCode());
    }

    [Fact(DisplayName = "Cart items differing in quantity are not equal")]
    public void Given_DifferentQuantity_When_Compared_Then_IsNotEqual()
    {
        var product = CartTestData.ProductRef();

        new CartItem(product, new Quantity(3)).Should().NotBe(new CartItem(product, new Quantity(4)));
    }

    [Fact(DisplayName = "Replacing the quantity leaves the original item untouched")]
    public void Given_Item_When_QuantityReplaced_Then_ReturnsNewInstance()
    {
        var item = CartTestData.Item(quantity: 2);

        var replaced = item.WithQuantity(new Quantity(9));

        replaced.Should().NotBeSameAs(item);
        replaced.Product.Should().Be(item.Product);
        replaced.Quantity.Value.Should().Be(9);
        item.Quantity.Value.Should().Be(2);
    }

    [Fact(DisplayName = "A cart item without a product reference throws")]
    public void Given_NoProduct_When_Created_Then_Throws()
    {
        var act = () => new CartItem(null!, new Quantity(1));

        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "A cart item without a quantity throws")]
    public void Given_NoQuantity_When_Created_Then_Throws()
    {
        var act = () => new CartItem(CartTestData.ProductRef(), null!);

        act.Should().Throw<DomainException>();
    }
}
