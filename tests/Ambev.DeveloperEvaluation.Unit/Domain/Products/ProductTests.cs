using Ambev.DeveloperEvaluation.Domain.Products;
using Ambev.DeveloperEvaluation.Domain.Products.Events;
using Ambev.DeveloperEvaluation.Domain.Products.ValueObjects;
using Ambev.DeveloperEvaluation.Unit.Domain.Products.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Products;

public class ProductTests
{
    [Fact]
    public void Creating_A_Product_From_Valid_Values_Populates_The_Aggregate()
    {
        // Arrange
        var title = ProductTestData.Title();
        var price = ProductTestData.Price();
        var category = ProductTestData.Category();

        // Act
        var product = Product.Create(title, price, "A description", category, ProductTestData.Image(), ProductTestData.Rating());

        // Assert
        product.Id.Should().NotBeEmpty();
        product.Title.Should().Be(title);
        product.Price.Should().Be(price);
        product.Category.Should().Be(category);
        product.Description.Should().Be("A description");
        product.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        product.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void Creating_A_Product_Raises_The_Created_Event()
    {
        // Act
        var product = ProductTestData.Product();

        // Assert
        product.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProductCreatedDomainEvent>()
            .Which.Title.Should().Be(product.Title.Value);
    }

    [Fact]
    public void Updating_The_Details_Raises_Exactly_One_Updated_Event()
    {
        // Arrange
        var product = ProductTestData.Product();
        product.ClearDomainEvents();

        // Act
        product.UpdateDetails(
            new ProductTitle("Renamed Backpack"),
            null,
            ProductTestData.Category(),
            ProductTestData.Image());

        // Assert
        product.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProductUpdatedDomainEvent>()
            .Which.Title.Should().Be("Renamed Backpack");
    }

    [Fact]
    public void Deleting_A_Product_Raises_The_Deleted_Event()
    {
        // Arrange
        var product = ProductTestData.Product();
        product.ClearDomainEvents();

        // Act
        product.Delete();

        // Assert
        product.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProductDeletedDomainEvent>()
            .Which.ProductId.Should().Be(product.Id);
    }

    [Fact]
    public void Repricing_A_Product_Raises_No_Event_Because_The_Replica_Only_Tracks_Titles()
    {
        // Arrange
        var product = ProductTestData.Product();
        product.ClearDomainEvents();

        // Act
        product.Reprice(new Money(12.34m));

        // Assert
        product.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Rating_A_Product_Raises_No_Event_Because_The_Replica_Only_Tracks_Titles()
    {
        // Arrange
        var product = ProductTestData.Product();
        product.ClearDomainEvents();

        // Act
        product.SetRating(new Rating(3.1m, 12));

        // Assert
        product.DomainEvents.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Creating_A_Product_Without_A_Description_Normalizes_It_To_Empty(string? description)
    {
        // Act
        var product = Product.Create(
            ProductTestData.Title(),
            ProductTestData.Price(),
            description,
            ProductTestData.Category(),
            ProductTestData.Image(),
            ProductTestData.Rating());

        // Assert
        product.Description.Should().BeEmpty();
    }

    [Fact]
    public void Creating_A_Product_Without_A_Required_Value_Object_Throws()
    {
        // Act
        var act = () => Product.Create(
            null!,
            ProductTestData.Price(),
            null,
            ProductTestData.Category(),
            ProductTestData.Image(),
            ProductTestData.Rating());

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Repricing_A_Product_Replaces_The_Price_And_Stamps_The_Update_Time()
    {
        // Arrange
        var product = ProductTestData.Product();
        var price = new Money(199.90m);

        // Act
        product.Reprice(price);

        // Assert
        product.Price.Should().Be(price);
        product.UpdatedAt.Should().NotBeNull().And.BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Rating_A_Product_Replaces_The_Rating_And_Stamps_The_Update_Time()
    {
        // Arrange
        var product = ProductTestData.Product();
        var rating = new Rating(4.2m, 200);

        // Act
        product.SetRating(rating);

        // Assert
        product.Rating.Should().Be(rating);
        product.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Updating_The_Details_Replaces_The_Descriptive_Fields_And_Keeps_The_Price()
    {
        // Arrange
        var product = ProductTestData.Product();
        var price = product.Price;
        var title = new ProductTitle("Updated Backpack");
        var category = new Category("Accessories");
        var image = new ImageUrl("https://example.test/updated.png");

        // Act
        product.UpdateDetails(title, "  Bigger bag  ", category, image);

        // Assert
        product.Title.Should().Be(title);
        product.Category.Should().Be(category);
        product.Image.Should().Be(image);
        product.Description.Should().Be("Bigger bag");
        product.Price.Should().Be(price);
        product.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Mutating_A_Product_With_A_Missing_Value_Object_Throws()
    {
        // Arrange
        var product = ProductTestData.Product();

        // Act
        var act = () => product.Reprice(null!);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Restoring_A_Product_Keeps_Its_Identity_And_Timestamps()
    {
        // Arrange
        var id = Guid.NewGuid();
        var createdAt = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var updatedAt = createdAt.AddDays(1);

        // Act
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

        // Assert
        product.Id.Should().Be(id);
        product.CreatedAt.Should().Be(createdAt);
        product.UpdatedAt.Should().Be(updatedAt);
    }
}
