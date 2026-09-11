using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Application.Carts.TestData;

public static class CatalogEventTestData
{
    private static readonly Faker Faker = new();

    public static Guid ProductId() => Faker.Random.Guid();

    public static string Title() => Faker.Commerce.ProductName();

    public static DateTime OccurredOn() => Faker.Date.RecentOffset().UtcDateTime;
}
