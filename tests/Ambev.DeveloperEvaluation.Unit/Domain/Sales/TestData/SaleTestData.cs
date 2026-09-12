using Ambev.DeveloperEvaluation.Domain.Sales.ValueObjects;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Sales.TestData;

public static class SaleTestData
{
    private static readonly Faker Faker = new();

    public static Guid Id() => Faker.Random.Guid();

    public static string PersonName() => Faker.Name.FullName();

    public static string BranchName() => $"{Faker.Address.City()} Store";

    public static string ProductTitle() => Faker.Commerce.ProductName();

    public static decimal Price() => Faker.Random.Decimal(1m, 500m);

    public static Money Amount(decimal? value = null) => new(value ?? Price());

    public static Quantity Quantity(int? value = null) => new(value ?? Faker.Random.Int(1, 20));

    public static CustomerRef Customer(Guid? id = null, string? name = null) =>
        new(id ?? Id(), name ?? PersonName());

    public static BranchRef Branch(Guid? id = null, string? name = null) =>
        new(id ?? Id(), name ?? BranchName());

    public static ProductRef Product(Guid? id = null, string? title = null) =>
        new(id ?? Id(), title ?? ProductTitle());
}
