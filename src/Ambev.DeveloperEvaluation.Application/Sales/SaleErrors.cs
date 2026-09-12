using Ambev.DeveloperEvaluation.Application.Common.Results;

namespace Ambev.DeveloperEvaluation.Application.Sales;

public static class SaleErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        "Sales.NotFound",
        "Sale not found",
        $"The sale with ID {id} does not exist");

    public static Error ItemNotFound(Guid saleId, Guid itemId) => Error.NotFound(
        "Sales.ItemNotFound",
        "Sale item not found",
        $"The sale with ID {saleId} has no item with ID {itemId}");

    public static Error AlreadyCancelled(Guid id) => Error.Conflict(
        "Sales.AlreadyCancelled",
        "Sale already cancelled",
        $"The sale with ID {id} was cancelled and can no longer be changed");

    public static Error ItemAlreadyCancelled(Guid itemId) => Error.Conflict(
        "Sales.ItemAlreadyCancelled",
        "Sale item already cancelled",
        $"The item with ID {itemId} was already cancelled");

    public static Error CartAlreadyCheckedOut(Guid cartId) => Error.Conflict(
        "Sales.CartAlreadyCheckedOut",
        "Cart already checked out",
        $"The cart with ID {cartId} was already turned into a sale");

    public static Error UnknownBranch(Guid branchId) => Error.Unprocessable(
        "Sales.UnknownBranch",
        "Unknown branch",
        $"The branch with ID {branchId} is not a registered branch");

    public static Error UnknownCart(Guid cartId) => Error.Unprocessable(
        "Sales.UnknownCart",
        "Unknown cart",
        $"The cart with ID {cartId} does not exist or does not belong to you");

    public static Error EmptyCart(Guid cartId) => Error.Unprocessable(
        "Sales.EmptyCart",
        "Empty cart",
        $"The cart with ID {cartId} has no items to sell");

    public static Error EmptySale(Guid id) => Error.Unprocessable(
        "Sales.EmptySale",
        "Empty sale",
        $"The sale with ID {id} would be left without a single active item");

    public static Error UnknownProduct(Guid productId) => Error.Unprocessable(
        "Sales.UnknownProduct",
        "Unknown product",
        $"The product with ID {productId} is no longer in the catalog");

    public static Error UnknownItem(Guid productId) => Error.Unprocessable(
        "Sales.UnknownItem",
        "Unknown sale item",
        $"The product with ID {productId} is not on this sale and cannot be added by an update");

    public static Error QuantityExceeded(int attempted, int maximum) => Error.Unprocessable(
        "Sales.QuantityExceeded",
        "Quantity above the limit",
        $"A sale line cannot hold more than {maximum} identical items, {attempted} were requested");

    public static Error Rejected(string detail) => Error.Unprocessable(
        "Sales.Rejected",
        "Sale rejected",
        detail);
}
