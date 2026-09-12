using Ambev.DeveloperEvaluation.Domain.Sales.Events;
using Ambev.DeveloperEvaluation.Sales.IntegrationEvents;

namespace Ambev.DeveloperEvaluation.Application.Sales.Events;

internal static class SoldItems
{
    public static SoldItem From(SaleEventItem item) => new(
        item.ItemId,
        item.ProductId,
        item.Title,
        item.Quantity,
        item.UnitPrice,
        item.Discount,
        item.Total);
}
