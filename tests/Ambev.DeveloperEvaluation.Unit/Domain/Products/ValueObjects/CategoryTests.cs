using Ambev.DeveloperEvaluation.Domain.Products.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Products.ValueObjects;

public class CategoryTests
{
    [Theory(DisplayName = "Invalid category is rejected on construction")]
    [InlineData("")]
    [InlineData("   ")]
    public void Given_InvalidValue_When_Constructed_Then_Throws(string value)
    {
        var act = () => new Category(value);

        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "Category longer than 100 characters is rejected")]
    public void Given_TooLongValue_When_Constructed_Then_Throws()
    {
        var act = () => new Category(new string('a', 101));

        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "Category is trimmed on construction and keeps its casing")]
    public void Given_PaddedValue_When_Constructed_Then_TrimsAndKeepsCasing()
    {
        new Category("  Mens Clothing  ").Name.Should().Be("Mens Clothing");
    }

    [Theory(DisplayName = "Categories differing only by casing or padding are equal")]
    [InlineData("Mens Clothing", "mens clothing")]
    [InlineData("Jewelery", "JEWELERY")]
    [InlineData("Electronics", "  electronics  ")]
    public void Given_DifferentCasing_When_Compared_Then_Equal(string left, string right)
    {
        var a = new Category(left);
        var b = new Category(right);

        a.Should().Be(b);
        (a == b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact(DisplayName = "Categories with different names are not equal")]
    public void Given_DifferentName_When_Compared_Then_NotEqual()
    {
        new Category("Jewelery").Should().NotBe(new Category("Electronics"));
    }
}
