using Ambev.DeveloperEvaluation.Application.Common.Results;

namespace Ambev.DeveloperEvaluation.Application.Carts;

public static class CartErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        "Carts.NotFound",
        "Cart not found",
        $"The cart with ID {id} does not exist");

    public static Error AlreadyCheckedOut(Guid id) => Error.Conflict(
        "Carts.AlreadyCheckedOut",
        "Cart already checked out",
        $"The cart with ID {id} was checked out and can no longer be changed");
}
