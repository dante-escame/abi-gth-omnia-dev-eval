using Ambev.DeveloperEvaluation.Domain.Sales.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Sales.ValueObjects;

public class SaleItemTotalsTests
{
    [Fact]
    public void Totals_Whose_Net_Does_Not_Equal_Gross_Minus_Discount_Throw()
    {
        // Act
        var act = () => new SaleItemTotals(new Money(40.00m), new Money(4.00m), new Money(35.00m));

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Totals_Whose_Discount_Is_Larger_Than_The_Gross_Throw()
    {
        // Act
        var act = () => new SaleItemTotals(new Money(10.00m), new Money(11.00m), new Money(0m));

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Totals_Missing_Any_Of_The_Three_Amounts_Throw()
    {
        // Arrange
        var zero = new Money(0m);

        // Act
        var withoutGross = () => new SaleItemTotals(null!, zero, zero);
        var withoutDiscount = () => new SaleItemTotals(zero, null!, zero);
        var withoutNet = () => new SaleItemTotals(zero, zero, null!);

        // Assert
        withoutGross.Should().Throw<DomainException>();
        withoutDiscount.Should().Throw<DomainException>();
        withoutNet.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(10.00, 1, 0, 10.00, 0, 10.00)]
    [InlineData(10.00, 3, 0, 30.00, 0, 30.00)]
    [InlineData(10.00, 4, 0.10, 40.00, 4.00, 36.00)]
    [InlineData(10.00, 10, 0.20, 100.00, 20.00, 80.00)]
    [InlineData(19.99, 5, 0.10, 99.95, 10.00, 89.95)]
    public void Building_Totals_Derives_Gross_Discount_And_Net(
        decimal unitPrice,
        int quantity,
        decimal rate,
        decimal expectedGross,
        decimal expectedDiscount,
        decimal expectedNet)
    {
        // Act
        var totals = SaleItemTotals.From(
            new Money(unitPrice),
            new Quantity(quantity),
            DiscountRate.FromValue(rate));

        // Assert
        totals.Gross.Amount.Should().Be(expectedGross);
        totals.Discount.Amount.Should().Be(expectedDiscount);
        totals.Net.Amount.Should().Be(expectedNet);
    }

    [Fact]
    public void Building_Totals_Without_A_Quantity_Or_A_Rate_Throws()
    {
        // Act
        var withoutQuantity = () => SaleItemTotals.From(new Money(1m), null!, DiscountRate.None);
        var withoutRate = () => SaleItemTotals.From(new Money(1m), new Quantity(1), null!);

        // Assert
        withoutQuantity.Should().Throw<DomainException>();
        withoutRate.Should().Throw<DomainException>();
    }

    [Fact]
    public void Two_Sets_Of_Totals_With_The_Same_Amounts_Are_Equal()
    {
        // Arrange
        var a = SaleItemTotals.From(new Money(10.00m), new Quantity(4), DiscountRate.TenPercent);

        // Act
        var b = new SaleItemTotals(new Money(40.00m), new Money(4.00m), new Money(36.00m));

        // Assert
        a.Should().Be(b);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }
}
