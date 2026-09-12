using Ambev.DeveloperEvaluation.Domain.Products.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Products.ValueObjects;

public class MoneyTests
{
    [Theory(DisplayName = "Amount that is not greater than zero is rejected")]
    [InlineData(0)]
    [InlineData(-0.01)]
    [InlineData(-100)]
    public void Given_NonPositiveAmount_When_Constructed_Then_Throws(decimal amount)
    {
        var act = () => new Money(amount);

        act.Should().Throw<DomainException>();
    }

    [Theory(DisplayName = "Positive amount is accepted")]
    [InlineData(0.01)]
    [InlineData(55.99)]
    [InlineData(695)]
    public void Given_PositiveAmount_When_Constructed_Then_KeepsAmount(decimal amount)
    {
        new Money(amount).Amount.Should().Be(amount);
    }

    [Fact(DisplayName = "Amounts with the same value are structurally equal")]
    public void Given_SameAmount_When_Compared_Then_Equal()
    {
        var a = new Money(55.99m);
        var b = new Money(55.99m);

        a.Should().Be(b);
        (a == b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact(DisplayName = "Amounts with different values are not equal")]
    public void Given_DifferentAmount_When_Compared_Then_NotEqual()
    {
        new Money(55.99m).Should().NotBe(new Money(22.30m));
    }
}
