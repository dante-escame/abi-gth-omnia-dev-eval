using Ambev.DeveloperEvaluation.Application.Common;
using Ambev.DeveloperEvaluation.Application.Ports;
using Ambev.DeveloperEvaluation.Domain.Carts;
using Ambev.DeveloperEvaluation.Domain.Carts.ValueObjects;

namespace Ambev.DeveloperEvaluation.Application.Carts.Common;

public static class CartLines
{
    public static async Task<List<CartItem>> BuildAsync(
        IReadOnlyList<CartLineInput> lines,
        IProductReader productReader,
        CancellationToken cancellationToken)
    {
        var ids = lines.Select(line => line.ProductId).Distinct().ToList();
        var products = await productReader.GetManyAsync(ids, cancellationToken);

        return lines
            .Select(line => new CartItem(
                new ProductRef(line.ProductId, products.GetValueOrDefault(line.ProductId)?.Title),
                new Quantity(line.Quantity)))
            .ToList();
    }

    public static bool IsVisibleTo(Cart cart, CallerContext caller) =>
        !caller.IsScoped || cart.CustomerId == caller.UserId;
}
