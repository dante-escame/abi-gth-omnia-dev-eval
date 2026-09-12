using Ambev.DeveloperEvaluation.Domain.Carts.ValueObjects;
using Ambev.DeveloperEvaluation.Unit.Domain.Carts.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Carts.ValueObjects;

public class ProductRefTests
{
    [Fact]
    public void Constructing_A_Product_Reference_Without_An_Id_Throws()
    {
        // Act
        var act = () => new ProductRef(Guid.Empty, CartTestData.Title());

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructing_A_Product_Reference_With_A_Blank_Title_Normalizes_It_To_Nothing(string? title)
    {
        // Act
        var reference = new ProductRef(CartTestData.ProductId(), title);

        // Assert
        reference.Title.Should().BeNull();
    }

    [Fact]
    public void Constructing_A_Product_Reference_Trims_The_Title()
    {
        // Act
        var reference = new ProductRef(CartTestData.ProductId(), "  Mountain Bike  ");

        // Assert
        reference.Title.Should().Be("Mountain Bike");
    }

    [Fact]
    public void A_Product_Reference_Without_A_Title_Is_Allowed()
    {
        // Arrange
        var id = CartTestData.ProductId();

        // Act
        var reference = new ProductRef(id);

        // Assert
        reference.Id.Should().Be(id);
        reference.Title.Should().BeNull();
    }

    [Fact]
    public void Two_Product_References_Compare_By_Id_And_Title()
    {
        // Arrange
        var id = CartTestData.ProductId();

        // Act
        var reference = new ProductRef(id, "Bike");

        // Assert
        reference.Should().Be(new ProductRef(id, "Bike"));
        reference.Should().NotBe(new ProductRef(id, "Scooter"));
        reference.Should().NotBe(new ProductRef(CartTestData.ProductId(), "Bike"));
    }
}
