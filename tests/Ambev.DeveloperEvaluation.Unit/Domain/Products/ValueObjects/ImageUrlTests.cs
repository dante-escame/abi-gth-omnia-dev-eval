using Ambev.DeveloperEvaluation.Domain.Products.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Products.ValueObjects;

public class ImageUrlTests
{
    [Theory(DisplayName = "Invalid image url is rejected on construction")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-a-url")]
    [InlineData("/relative/path.png")]
    [InlineData("example.test/image.png")]
    public void Given_InvalidValue_When_Constructed_Then_Throws(string value)
    {
        var act = () => new ImageUrl(value);

        act.Should().Throw<DomainException>();
    }

    [Theory(DisplayName = "Absolute url is accepted")]
    [InlineData("https://example.test/image.png")]
    [InlineData("http://cdn.example.test/a/b/c.jpg?v=2")]
    public void Given_AbsoluteUrl_When_Constructed_Then_KeepsValue(string value)
    {
        new ImageUrl(value).Value.Should().Be(value);
    }

    [Fact(DisplayName = "Image url is trimmed on construction")]
    public void Given_PaddedValue_When_Constructed_Then_Trims()
    {
        new ImageUrl("  https://example.test/a.png  ").Value.Should().Be("https://example.test/a.png");
    }

    [Fact(DisplayName = "Image urls with the same value are structurally equal")]
    public void Given_SameValue_When_Compared_Then_Equal()
    {
        var a = new ImageUrl("https://example.test/a.png");
        var b = new ImageUrl("https://example.test/a.png");

        a.Should().Be(b);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact(DisplayName = "Image urls with different values are not equal")]
    public void Given_DifferentValue_When_Compared_Then_NotEqual()
    {
        new ImageUrl("https://example.test/a.png").Should().NotBe(new ImageUrl("https://example.test/b.png"));
    }
}
