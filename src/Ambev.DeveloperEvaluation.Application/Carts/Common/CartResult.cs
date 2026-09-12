using Ambev.DeveloperEvaluation.Domain.Carts;

namespace Ambev.DeveloperEvaluation.Application.Carts.Common;

public sealed record CartLineResult(Guid ProductId, string? Title, int Quantity);

public sealed record CartResult(Guid Id, Guid UserId, DateTime Date, IReadOnlyList<CartLineResult> Products)
{
    public static CartResult From(Cart cart) => new(
        cart.Id,
        cart.CustomerId,
        cart.CreatedAt,
        cart.Items
            .Select(item => new CartLineResult(item.Product.Id, item.Product.Title, item.Quantity.Value))
            .ToList());

    public static CartResult From(CartListItem item) => new(
        item.Id,
        item.UserId,
        item.Date,
        item.Items
            .Select(line => new CartLineResult(line.ProductId, line.Title, line.Quantity))
            .ToList());
}
