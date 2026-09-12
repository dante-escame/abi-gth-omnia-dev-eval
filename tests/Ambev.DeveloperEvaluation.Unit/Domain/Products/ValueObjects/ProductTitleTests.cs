using Ambev.DeveloperEvaluation.Domain.Products.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Products.ValueObjects;

public class ProductTitleTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructing_A_Product_Title_From_A_Blank_Value_Throws(string value)
    {
        // Act
        var act = () => new ProductTitle(value);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructing_A_Product_Title_Longer_Than_200_Characters_Throws()
    {
        // Act
        var act = () => new ProductTitle(new string('a', 201));

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructing_A_Product_Title_Of_Exactly_200_Characters_Is_Allowed()
    {
        // Act
        var title = new ProductTitle(new string('a', 200));

        // Assert
        title.Value.Should().HaveLength(200);
    }

    [Fact]
    public void Constructing_A_Product_Title_Trims_The_Padding()
    {
        // Act
        var title = new ProductTitle("  Mens Cotton Jacket  ");

        // Assert
        title.Value.Should().Be("Mens Cotton Jacket");
    }

    [Fact]
    public void Two_Product_Titles_With_The_Same_Value_Are_Equal()
    {
        // Arrange
        var a = new ProductTitle("Backpack");
        var b = new ProductTitle("Backpack");

        // Act
        bool equal = a == b;

        // Assert
        a.Should().Be(b);
        equal.Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Two_Product_Titles_With_Different_Values_Are_Not_Equal()
    {
        // Arrange
        var a = new ProductTitle("Backpack");

        // Act
        var b = new ProductTitle("Jacket");

        // Assert
        a.Should().NotBe(b);
    }
}
