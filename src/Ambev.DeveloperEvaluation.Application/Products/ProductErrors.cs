using Ambev.DeveloperEvaluation.Application.Common.Results;

namespace Ambev.DeveloperEvaluation.Application.Products;

public static class ProductErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        "Products.NotFound",
        "Product not found",
        $"The product with ID {id} does not exist in our catalog");
}
