using Ambev.DeveloperEvaluation.Domain.Sales;
using Ambev.DeveloperEvaluation.Domain.Sales.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Sales.ValueObjects;

public class QuantityTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructing_A_Quantity_Below_One_Throws(int value)
    {
        // Act
        var act = () => new Quantity(value);

        // Assert
        var exception = act.Should().Throw<DomainException>().Which;
        exception.Should().NotBeOfType<MaxItemsExceededException>();
    }

    [Theory]
    [InlineData(21)]
    [InlineData(100)]
    public void Constructing_A_Quantity_Above_Twenty_Throws_The_Item_Cap(int value)
    {
        // Act
        var act = () => new Quantity(value);

        // Assert
        var exception = act.Should().Throw<MaxItemsExceededException>().Which;
        exception.Attempted.Should().Be(value);
        exception.Maximum.Should().Be(Quantity.Maximum);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(4)]
    [InlineData(20)]
    public void A_Quantity_Inside_The_Band_Keeps_Its_Value(int value)
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
        var sum = left.Add(new Quantity(6));

        // Assert
        sum.Value.Should().Be(10);
        left.Value.Should().Be(4);
    }

    [Fact]
    public void Adding_Two_Quantities_Past_The_Cap_Throws_The_Item_Cap()
    {
        // Arrange
        var left = new Quantity(20);

        // Act
        var act = () => left.Add(new Quantity(1));

        // Assert
        act.Should().Throw<MaxItemsExceededException>();
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
