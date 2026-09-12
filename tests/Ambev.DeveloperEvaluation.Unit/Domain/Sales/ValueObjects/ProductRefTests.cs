using Ambev.DeveloperEvaluation.Domain.Sales.ValueObjects;
using Ambev.DeveloperEvaluation.Unit.Domain.Sales.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Sales.ValueObjects;

public class ProductRefTests
{
    [Fact]
    public void Constructing_A_Product_Reference_Without_An_Id_Throws()
    {
        // Act
        var act = () => new ProductRef(Guid.Empty, SaleTestData.ProductTitle());

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void A_Sold_Product_Reference_Requires_A_Title_Unlike_The_Cart_One(string title)
    {
        // Act
        var act = () => new ProductRef(SaleTestData.Id(), title);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructing_A_Product_Reference_Trims_The_Title()
    {
        // Act
        var product = new ProductRef(SaleTestData.Id(), "  Helmet  ");

        // Assert
        product.Title.Should().Be("Helmet");
    }

    [Fact]
    public void Two_Product_References_Compare_By_Id_And_Title()
    {
        // Arrange
        var id = SaleTestData.Id();

        // Act
        var product = new ProductRef(id, "Helmet");

        // Assert
        product.Should().Be(new ProductRef(id, "Helmet"));
        product.Should().NotBe(new ProductRef(id, "Gloves"));
        product.Should().NotBe(new ProductRef(SaleTestData.Id(), "Helmet"));
    }
}
