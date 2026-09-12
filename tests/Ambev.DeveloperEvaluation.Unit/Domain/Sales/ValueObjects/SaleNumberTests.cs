using Ambev.DeveloperEvaluation.Domain.Sales.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Sales.ValueObjects;

public class SaleNumberTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("SALE-123")]
    [InlineData("SALE000123")]
    [InlineData("sale-000123")]
    [InlineData("000123")]
    [InlineData("SALE-0001A3")]
    public void Constructing_A_Sale_Number_From_A_Malformed_Value_Throws(string value)
    {
        // Act
        var act = () => new SaleNumber(value);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("SALE-000001")]
    [InlineData("SALE-123456")]
    [InlineData("SALE-1234567")]
    public void Constructing_A_Sale_Number_From_A_Valid_Value_Keeps_It(string value)
    {
        // Act
        var number = new SaleNumber(value);

        // Assert
        number.Value.Should().Be(value);
    }

    [Fact]
    public void A_Sale_Number_Is_Trimmed_Before_It_Is_Validated()
    {
        // Act
        var number = new SaleNumber("  SALE-000123  ");

        // Assert
        number.Value.Should().Be("SALE-000123");
    }

    [Theory]
    [InlineData(1, "SALE-000001")]
    [InlineData(123, "SALE-000123")]
    [InlineData(999999, "SALE-999999")]
    [InlineData(1000000, "SALE-1000000")]
    public void A_Sequence_Value_Is_Padded_To_At_Least_Six_Digits(long sequence, string expected)
    {
        // Act
        var number = SaleNumber.FromSequence(sequence);

        // Assert
        number.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void A_Sequence_Value_That_Is_Not_Positive_Throws(long sequence)
    {
        // Act
        var act = () => SaleNumber.FromSequence(sequence);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Two_Sale_Numbers_With_The_Same_Value_Are_Equal()
    {
        // Arrange
        var a = new SaleNumber("SALE-000123");

        // Act
        var b = SaleNumber.FromSequence(123);

        // Assert
        a.Should().Be(b);
        a.Should().NotBe(SaleNumber.FromSequence(124));
        a.GetHashCode().Should().Be(b.GetHashCode());
    }
}
