using Ambev.DeveloperEvaluation.Domain.Products.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Products.ValueObjects;

public class MoneyTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    [InlineData(-100)]
    public void Constructing_Money_From_An_Amount_That_Is_Not_Positive_Throws(decimal amount)
    {
        // Act
        var act = () => new Money(amount);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(55.99)]
    [InlineData(695)]
    public void Constructing_Money_From_A_Positive_Amount_Keeps_The_Amount(decimal amount)
    {
        // Act
        var money = new Money(amount);

        // Assert
        money.Amount.Should().Be(amount);
    }

    [Fact]
    public void Two_Amounts_With_The_Same_Value_Are_Equal()
    {
        // Arrange
        var a = new Money(55.99m);
        var b = new Money(55.99m);

        // Act
        bool equal = a == b;

        // Assert
        a.Should().Be(b);
        equal.Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Two_Amounts_With_Different_Values_Are_Not_Equal()
    {
        // Arrange
        var a = new Money(55.99m);

        // Act
        var b = new Money(22.30m);

        // Assert
        a.Should().NotBe(b);
    }
}
