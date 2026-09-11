using Ambev.DeveloperEvaluation.Domain.Products.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Products.ValueObjects;

public class CategoryTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructing_A_Category_From_A_Blank_Value_Throws(string value)
    {
        // Act
        var act = () => new Category(value);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructing_A_Category_Longer_Than_100_Characters_Throws()
    {
        // Act
        var act = () => new Category(new string('a', 101));

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructing_A_Category_Trims_The_Padding_And_Keeps_The_Casing()
    {
        // Act
        var category = new Category("  Mens Clothing  ");

        // Assert
        category.Name.Should().Be("Mens Clothing");
    }

    [Theory]
    [InlineData("Mens Clothing", "mens clothing")]
    [InlineData("Jewelery", "JEWELERY")]
    [InlineData("Electronics", "  electronics  ")]
    public void Two_Categories_Differing_Only_By_Casing_Or_Padding_Are_Equal(string left, string right)
    {
        // Arrange
        var a = new Category(left);
        var b = new Category(right);

        // Act
        bool equal = a == b;

        // Assert
        a.Should().Be(b);
        equal.Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Two_Categories_With_Different_Names_Are_Not_Equal()
    {
        // Arrange
        var a = new Category("Jewelery");

        // Act
        var b = new Category("Electronics");

        // Assert
        a.Should().NotBe(b);
    }
}
