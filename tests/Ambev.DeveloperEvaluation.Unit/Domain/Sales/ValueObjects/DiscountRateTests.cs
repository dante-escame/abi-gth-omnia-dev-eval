using Ambev.DeveloperEvaluation.Domain.Sales.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Sales.ValueObjects;

public class DiscountRateTests
{
    [Theory]
    [InlineData(0.05)]
    [InlineData(0.15)]
    [InlineData(0.30)]
    [InlineData(1)]
    [InlineData(-0.10)]
    public void Asking_For_A_Rate_Outside_The_Tier_Table_Throws(decimal value)
    {
        // Act
        var act = () => DiscountRate.FromValue(value);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void The_Three_Supported_Rates_Round_Trip_Through_Their_Value()
    {
        // Act
        var none = DiscountRate.FromValue(0m);
        var ten = DiscountRate.FromValue(0.10m);
        var twenty = DiscountRate.FromValue(0.20m);

        // Assert
        none.Should().Be(DiscountRate.None);
        ten.Should().Be(DiscountRate.TenPercent);
        twenty.Should().Be(DiscountRate.TwentyPercent);
    }

    [Fact]
    public void A_Caller_Has_No_Public_Constructor_To_Invent_A_Rate_With()
    {
        // Act
        var constructors = typeof(DiscountRate).GetConstructors();

        // Assert
        constructors.Should().BeEmpty();
    }

    [Fact]
    public void Two_Rates_With_The_Same_Value_Are_Equal()
    {
        // Act
        var rate = DiscountRate.FromValue(0.20m);

        // Assert
        rate.Should().Be(DiscountRate.TwentyPercent);
        rate.Should().NotBe(DiscountRate.TenPercent);
        rate.GetHashCode().Should().Be(DiscountRate.TwentyPercent.GetHashCode());
    }
}
