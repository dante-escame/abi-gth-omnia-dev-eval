using Ambev.DeveloperEvaluation.Application.Common.Lists;

namespace Ambev.DeveloperEvaluation.Application.Sales.Common;

public static class SaleListFields
{
    public static readonly ListFieldMap<SaleListItem> Map = new ListFieldMap<SaleListItem>()
        .Map("id", sale => sale.Id)
        .Map("saleNumber", sale => sale.SaleNumber)
        .Map("date", sale => sale.Date)
        .Map("status", sale => sale.Status)
        .Map("customerId", sale => sale.CustomerId)
        .Map("branchId", sale => sale.BranchId)
        .Map("total", sale => sale.Total);
}
