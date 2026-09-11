using Ambev.DeveloperEvaluation.Domain.Products.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Products.ValueObjects;

public class ProductTitleTests
{
    [Theory(DisplayName = "Invalid title is rejected on construction")]
    [InlineData("")]
    [InlineData("   ")]
    public void Given_InvalidValue_When_Constructed_Then_Throws(string value)
    {
        var act = () => new ProductTitle(value);

        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "Title longer than 200 characters is rejected")]
    public void Given_TooLongValue_When_Constructed_Then_Throws()
    {
        var act = () => new ProductTitle(new string('a', 201));

        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "Title of exactly 200 characters is accepted")]
    public void Given_MaximumLength_When_Constructed_Then_KeepsValue()
    {
        new ProductTitle(new string('a', 200)).Value.Should().HaveLength(200);
    }

    [Fact(DisplayName = "Title is trimmed on construction")]
    public void Given_PaddedValue_When_Constructed_Then_Trims()
    {
        new ProductTitle("  Mens Cotton Jacket  ").Value.Should().Be("Mens Cotton Jacket");
    }

    [Fact(DisplayName = "Titles with the same value are structurally equal")]
    public void Given_SameValue_When_Compared_Then_Equal()
    {
        var a = new ProductTitle("Backpack");
        var b = new ProductTitle("Backpack");

        a.Should().Be(b);
        (a == b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact(DisplayName = "Titles with different values are not equal")]
    public void Given_DifferentValue_When_Compared_Then_NotEqual()
    {
        new ProductTitle("Backpack").Should().NotBe(new ProductTitle("Jacket"));
    }
}
