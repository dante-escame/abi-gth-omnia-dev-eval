using Ambev.DeveloperEvaluation.Domain.Carts.ValueObjects;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Bogus;
using CartAggregate = Ambev.DeveloperEvaluation.Domain.Carts.Cart;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Carts.TestData;

public static class CartTestData
{
    private static readonly Faker Faker = new();

    public static Guid CustomerId() => Faker.Random.Guid();

    public static Guid ProductId() => Faker.Random.Guid();

    public static string Title() => Faker.Commerce.ProductName();

    public static ProductRef ProductRef(Guid? id = null, string? title = null) =>
        new(id ?? ProductId(), title ?? Title());

    public static Quantity Quantity(int? value = null) => new(value ?? Faker.Random.Int(1, 50));

    public static CartItem Item(Guid? productId = null, string? title = null, int? quantity = null) =>
        new(ProductRef(productId, title), Quantity(quantity));

    public static CartAggregate Cart(params CartItem[] items) => CartFor(CustomerId(), items);

    public static CartAggregate CartFor(Guid customerId, params CartItem[] items) =>
        CartAggregate.Create(customerId, Lines(items));

    public static CartAggregate RestoredCart(CartStatus status, params CartItem[] items) =>
        RestoredCartFor(status, CustomerId(), items);

    public static CartAggregate RestoredCartFor(CartStatus status, Guid customerId, params CartItem[] items) =>
        CartAggregate.Restore(Faker.Random.Guid(), customerId, status, Lines(items), DateTime.UtcNow.AddHours(-1), null);

    private static CartItem[] Lines(CartItem[] items) => items.Length > 0 ? items : [Item()];
}
