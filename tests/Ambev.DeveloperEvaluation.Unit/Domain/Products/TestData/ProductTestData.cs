using Ambev.DeveloperEvaluation.Domain.Products;
using Ambev.DeveloperEvaluation.Domain.Products.ValueObjects;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Products.TestData;

public static class ProductTestData
{
    private static readonly Faker Faker = new();

    public static ProductTitle Title() => new(Faker.Commerce.ProductName());

    public static Money Price() => new(Faker.Random.Decimal(1m, 5000m));

    public static Category Category() => new(Faker.Commerce.Department());

    public static ImageUrl Image() => new(Faker.Image.PicsumUrl());

    public static Rating Rating() => new(Faker.Random.Decimal(0m, 5m), Faker.Random.Int(0, 10000));

    public static Product Product() => global::Ambev.DeveloperEvaluation.Domain.Products.Product.Create(
        Title(),
        Price(),
        Faker.Commerce.ProductDescription(),
        Category(),
        Image(),
        Rating());
}
