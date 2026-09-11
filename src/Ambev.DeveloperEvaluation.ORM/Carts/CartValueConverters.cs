using Ambev.DeveloperEvaluation.Domain.Carts.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Ambev.DeveloperEvaluation.ORM.Carts;

public static class CartValueConverters
{
    public static readonly ValueConverter<Quantity, int> QuantityConverter =
        new(quantity => quantity.Value, value => new Quantity(value));
}
