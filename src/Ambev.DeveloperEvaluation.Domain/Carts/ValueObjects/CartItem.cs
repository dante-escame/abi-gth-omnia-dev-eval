using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Carts.ValueObjects;

public sealed class CartItem : ValueObject
{
    public ProductRef Product { get; }

    public Quantity Quantity { get; }

    private CartItem()
    {
        Product = null!;
        Quantity = null!;
    }

    public CartItem(ProductRef product, Quantity quantity)
    {
        Product = product ?? throw new DomainException("Cart item requires a product reference.");
        Quantity = quantity ?? throw new DomainException("Cart item requires a quantity.");
    }

    public CartItem WithQuantity(Quantity quantity) => new(Product, quantity);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Product;
        yield return Quantity;
    }
}
