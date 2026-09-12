using Ambev.DeveloperEvaluation.Domain.Sales.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Ambev.DeveloperEvaluation.ORM.Sales;

public static class SaleValueConverters
{
    public const string AmountColumnType = "numeric(18,2)";

    public static readonly ValueConverter<Money, decimal> MoneyConverter =
        new(money => money.Amount, amount => new Money(amount));

    public static readonly ValueConverter<Quantity, int> QuantityConverter =
        new(quantity => quantity.Value, value => new Quantity(value));

    public static readonly ValueConverter<DiscountRate, decimal> DiscountRateConverter =
        new(rate => rate.Value, value => DiscountRate.FromValue(value));

    public static readonly ValueConverter<SaleNumber, string> SaleNumberConverter =
        new(number => number.Value, value => new SaleNumber(value));
}
