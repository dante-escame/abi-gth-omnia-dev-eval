using Ambev.DeveloperEvaluation.Domain.Products;
using Ambev.DeveloperEvaluation.Domain.Products.ValueObjects;
using Ambev.DeveloperEvaluation.Unit.Domain.Products.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Products;

public class ProductTests
{
    [Fact(DisplayName = "Creating a product produces a valid aggregate")]
    public void Given_ValidValues_When_Created_Then_IsPopulated()
    {
        var title = ProductTestData.Title();
        var price = ProductTestData.Price();
        var category = ProductTestData.Category();

        var product = Product.Create(title, price, "A description", category, ProductTestData.Image(), ProductTestData.Rating());

        product.Id.Should().NotBeEmpty();
        product.Title.Should().Be(title);
        product.Price.Should().Be(price);
        product.Category.Should().Be(category);
        product.Description.Should().Be("A description");
        product.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        product.UpdatedAt.Should().BeNull();
    }

    [Fact(DisplayName = "Creating a product raises no domain event")]
    public void Given_NewProduct_When_Created_Then_RaisesNothing()
    {
        ProductTestData.Product().DomainEvents.Should().BeEmpty();
    }

    [Theory(DisplayName = "Missing description is normalized to empty")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Given_BlankDescription_When_Created_Then_IsEmpty(string? description)
    {
        var product = Product.Create(
            ProductTestData.Title(),
            ProductTestData.Price(),
            description,
            ProductTestData.Category(),
            ProductTestData.Image(),
            ProductTestData.Rating());

        product.Description.Should().BeEmpty();
    }

    [Fact(DisplayName = "Creating a product without a required value object throws")]
    public void Given_MissingValueObject_When_Created_Then_Throws()
    {
        var act = () => Product.Create(
            null!,
            ProductTestData.Price(),
            null,
            ProductTestData.Category(),
            ProductTestData.Image(),
            ProductTestData.Rating());

        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "Repricing replaces the price and stamps the update time")]
    public void Given_Product_When_Repriced_Then_UpdatesPriceAndTimestamp()
    {
        var product = ProductTestData.Product();
        var price = new Money(199.90m);

        product.Reprice(price);

        product.Price.Should().Be(price);
        product.UpdatedAt.Should().NotBeNull().And.BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact(DisplayName = "Setting a rating replaces it and stamps the update time")]
    public void Given_Product_When_RatingSet_Then_UpdatesRatingAndTimestamp()
    {
        var product = ProductTestData.Product();
        var rating = new Rating(4.2m, 200);

        product.SetRating(rating);

        product.Rating.Should().Be(rating);
        product.UpdatedAt.Should().NotBeNull();
    }

    [Fact(DisplayName = "Updating details replaces the descriptive fields and keeps the price")]
    public void Given_Product_When_DetailsUpdated_Then_KeepsPrice()
    {
        var product = ProductTestData.Product();
        var price = product.Price;
        var title = new ProductTitle("Updated Backpack");
        var category = new Category("Accessories");
        var image = new ImageUrl("https://example.test/updated.png");

        product.UpdateDetails(title, "  Bigger bag  ", category, image);

        product.Title.Should().Be(title);
        product.Category.Should().Be(category);
        product.Image.Should().Be(image);
        product.Description.Should().Be("Bigger bag");
        product.Price.Should().Be(price);
        product.UpdatedAt.Should().NotBeNull();
    }

    [Fact(DisplayName = "Mutating with a missing value object throws")]
    public void Given_Product_When_RepricedWithNothing_Then_Throws()
    {
        var product = ProductTestData.Product();

        var act = () => product.Reprice(null!);

        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "Restoring a product keeps its identity and timestamps")]
    public void Given_StoredValues_When_Restored_Then_KeepsIdentity()
    {
        var id = Guid.NewGuid();
        var createdAt = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var updatedAt = createdAt.AddDays(1);

        var product = Product.Restore(
            id,
            ProductTestData.Title(),
            ProductTestData.Price(),
            null,
            ProductTestData.Category(),
            ProductTestData.Image(),
            ProductTestData.Rating(),
            createdAt,
            updatedAt);

        product.Id.Should().Be(id);
        product.CreatedAt.Should().Be(createdAt);
        product.UpdatedAt.Should().Be(updatedAt);
    }
}
