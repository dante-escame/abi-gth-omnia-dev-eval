using Ambev.DeveloperEvaluation.Domain.Carts.ValueObjects;
using Ambev.DeveloperEvaluation.Unit.Domain.Carts.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Carts.ValueObjects;

public class ProductRefTests
{
    [Fact(DisplayName = "A product reference without an id is rejected")]
    public void Given_EmptyId_When_Created_Then_Throws()
    {
        var act = () => new ProductRef(Guid.Empty, CartTestData.Title());

        act.Should().Throw<DomainException>();
    }

    [Theory(DisplayName = "A blank title is normalized to nothing")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Given_BlankTitle_When_Created_Then_TitleIsNull(string? title)
    {
        new ProductRef(CartTestData.ProductId(), title).Title.Should().BeNull();
    }

    [Fact(DisplayName = "A title keeps its text without the surrounding spaces")]
    public void Given_PaddedTitle_When_Created_Then_IsTrimmed()
    {
        new ProductRef(CartTestData.ProductId(), "  Mountain Bike  ").Title.Should().Be("Mountain Bike");
    }

    [Fact(DisplayName = "A reference with no title is allowed")]
    public void Given_NoTitle_When_Created_Then_KeepsId()
    {
        var id = CartTestData.ProductId();

        var reference = new ProductRef(id);

        reference.Id.Should().Be(id);
        reference.Title.Should().BeNull();
    }

    [Fact(DisplayName = "Product references compare by id and title")]
    public void Given_SameComponents_When_Compared_Then_IsEqual()
    {
        var id = CartTestData.ProductId();

        new ProductRef(id, "Bike").Should().Be(new ProductRef(id, "Bike"));
        new ProductRef(id, "Bike").Should().NotBe(new ProductRef(id, "Scooter"));
        new ProductRef(id, "Bike").Should().NotBe(new ProductRef(CartTestData.ProductId(), "Bike"));
    }
}
