using Ambev.DeveloperEvaluation.Domain.Carts.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Carts.ValueObjects;

public class QuantityTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-42)]
    public void Constructing_A_Quantity_Below_One_Throws(int value)
    {
        // Act
        var act = () => new Quantity(value);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(20)]
    [InlineData(21)]
    [InlineData(5000)]
    public void A_Quantity_Has_No_Upper_Bound_Because_The_Cap_Belongs_To_Sales(int value)
    {
        // Act
        var quantity = new Quantity(value);

        // Assert
        quantity.Value.Should().Be(value);
    }

    [Fact]
    public void Adding_Two_Quantities_Sums_Them_Into_A_New_Instance()
    {
        // Arrange
        var left = new Quantity(4);

        // Act
        var sum = left.Add(new Quantity(9));

        // Assert
        sum.Value.Should().Be(13);
        left.Value.Should().Be(4);
    }

    [Fact]
    public void Adding_A_Quantity_That_Is_Missing_Throws()
    {
        // Arrange
        var quantity = new Quantity(1);

        // Act
        var act = () => quantity.Add(null!);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Adding_Past_The_Integer_Ceiling_Throws()
    {
        // Arrange
        var quantity = new Quantity(int.MaxValue);

        // Act
        var act = () => quantity.Add(new Quantity(1));

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Two_Quantities_With_The_Same_Value_Are_Equal()
    {
        // Arrange
        var a = new Quantity(7);

        // Act
        var b = new Quantity(7);

        // Assert
        a.Should().Be(b);
        a.Should().NotBe(new Quantity(8));
        a.GetHashCode().Should().Be(b.GetHashCode());
    }
}
