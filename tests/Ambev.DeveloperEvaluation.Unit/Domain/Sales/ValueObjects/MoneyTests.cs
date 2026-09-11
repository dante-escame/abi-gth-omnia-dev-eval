using Ambev.DeveloperEvaluation.Domain.Sales.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Sales.ValueObjects;

public class MoneyTests
{
    [Theory]
    [InlineData(-0.01)]
    [InlineData(-1)]
    [InlineData(-999.99)]
    public void Constructing_An_Amount_Below_Zero_Throws(decimal amount)
    {
        // Act
        var act = () => new Money(amount);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void An_Amount_Of_Zero_Is_Legal_Because_A_Discount_Can_Be_Nothing()
    {
        // Act
        var money = new Money(0m);

        // Assert
        money.Amount.Should().Be(0m);
        money.Should().Be(Money.Zero);
    }

    [Theory]
    [InlineData(10.005, 10.01)]
    [InlineData(10.004, 10.00)]
    [InlineData(0.125, 0.13)]
    [InlineData(2.345, 2.35)]
    public void Constructing_An_Amount_Rounds_To_Two_Decimals_Away_From_Zero(decimal raw, decimal expected)
    {
        // Act
        var money = new Money(raw);

        // Assert
        money.Amount.Should().Be(expected);
    }

    [Fact]
    public void Adding_Two_Amounts_Produces_Their_Sum()
    {
        // Act
        var total = new Money(36.00m) + new Money(40.00m);

        // Assert
        total.Amount.Should().Be(76.00m);
    }

    [Fact]
    public void Subtracting_A_Discount_From_A_Gross_Produces_The_Net()
    {
        // Act
        var net = new Money(40.00m) - new Money(4.00m);

        // Assert
        net.Amount.Should().Be(36.00m);
    }

    [Fact]
    public void Subtracting_Past_Zero_Throws()
    {
        // Act
        var act = () => new Money(1.00m) - new Money(1.01m);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Multiplying_By_A_Quantity_Produces_The_Gross()
    {
        // Act
        var gross = new Money(10.00m) * 4;

        // Assert
        gross.Amount.Should().Be(40.00m);
    }

    [Fact]
    public void Multiplying_By_A_Rate_Rounds_The_Discount_On_The_Totals_Path()
    {
        // Act
        var discount = new Money(19.99m) * 3 * 0.10m;

        // Assert
        discount.Amount.Should().Be(6.00m);
    }

    [Fact]
    public void An_Operator_Given_A_Missing_Amount_Throws()
    {
        // Arrange
        var money = new Money(1.00m);

        // Act
        var add = () => money + null!;
        var subtract = () => money - null!;

        // Assert
        add.Should().Throw<DomainException>();
        subtract.Should().Throw<DomainException>();
    }

    [Fact]
    public void Two_Amounts_With_The_Same_Value_Are_Equal()
    {
        // Arrange
        var a = new Money(12.30m);

        // Act
        var b = new Money(12.304m);

        // Assert
        a.Should().Be(b);
        a.Should().NotBe(new Money(12.31m));
        a.GetHashCode().Should().Be(b.GetHashCode());
    }
}
