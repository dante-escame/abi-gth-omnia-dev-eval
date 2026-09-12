using Ambev.DeveloperEvaluation.Domain.Products.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Products.ValueObjects;

public class ImageUrlTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-a-url")]
    [InlineData("/relative/path.png")]
    [InlineData("example.test/image.png")]
    public void Constructing_An_Image_Url_From_An_Invalid_Value_Throws(string value)
    {
        // Act
        var act = () => new ImageUrl(value);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("https://example.test/image.png")]
    [InlineData("http://cdn.example.test/a/b/c.jpg?v=2")]
    public void Constructing_An_Image_Url_From_An_Absolute_Url_Keeps_The_Value(string value)
    {
        // Act
        var image = new ImageUrl(value);

        // Assert
        image.Value.Should().Be(value);
    }

    [Fact]
    public void Constructing_An_Image_Url_Trims_The_Padding()
    {
        // Act
        var image = new ImageUrl("  https://example.test/a.png  ");

        // Assert
        image.Value.Should().Be("https://example.test/a.png");
    }

    [Fact]
    public void Two_Image_Urls_With_The_Same_Value_Are_Equal()
    {
        // Arrange
        var a = new ImageUrl("https://example.test/a.png");

        // Act
        var b = new ImageUrl("https://example.test/a.png");

        // Assert
        a.Should().Be(b);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Two_Image_Urls_With_Different_Values_Are_Not_Equal()
    {
        // Arrange
        var a = new ImageUrl("https://example.test/a.png");

        // Act
        var b = new ImageUrl("https://example.test/b.png");

        // Assert
        a.Should().NotBe(b);
    }
}
