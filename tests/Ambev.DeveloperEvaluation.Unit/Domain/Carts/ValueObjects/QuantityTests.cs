using Ambev.DeveloperEvaluation.Domain.Carts.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Carts.ValueObjects;

public class QuantityTests
{
    [Theory(DisplayName = "A quantity below one is rejected")]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-42)]
    public void Given_BelowOne_When_Created_Then_Throws(int value)
    {
        var act = () => new Quantity(value);

        act.Should().Throw<DomainException>();
    }

    [Theory(DisplayName = "A quantity has no upper bound")]
    [InlineData(1)]
    [InlineData(20)]
    [InlineData(21)]
    [InlineData(5000)]
    public void Given_AtLeastOne_When_Created_Then_KeepsValue(int value)
    {
        new Quantity(value).Value.Should().Be(value);
    }

    [Fact(DisplayName = "Adding two quantities sums them into a new instance")]
    public void Given_TwoQuantities_When_Added_Then_Sums()
    {
        var left = new Quantity(4);

        var sum = left.Add(new Quantity(9));

        sum.Value.Should().Be(13);
        left.Value.Should().Be(4);
    }

    [Fact(DisplayName = "Adding nothing throws")]
    public void Given_NoOperand_When_Added_Then_Throws()
    {
        var act = () => new Quantity(1).Add(null!);

        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "Adding past the integer ceiling throws")]
    public void Given_Overflow_When_Added_Then_Throws()
    {
        var act = () => new Quantity(int.MaxValue).Add(new Quantity(1));

        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "Quantities compare by value")]
    public void Given_SameValue_When_Compared_Then_IsEqual()
    {
        new Quantity(7).Should().Be(new Quantity(7));
        new Quantity(7).Should().NotBe(new Quantity(8));
        new Quantity(7).GetHashCode().Should().Be(new Quantity(7).GetHashCode());
    }
}
