using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Carts.ValueObjects;

public sealed class CartItem(ProductRef product, Quantity quantity) : ValueObject
{
    public ProductRef Product { get; } = product ?? throw new DomainException("Cart item requires a product reference.");

    public Quantity Quantity { get; } = quantity ?? throw new DomainException("Cart item requires a quantity.");

    public CartItem WithQuantity(Quantity quantity) => new(Product, quantity);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Product;
        yield return Quantity;
    }
}
