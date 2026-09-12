using System.Linq.Expressions;
using Ambev.DeveloperEvaluation.Application.Ports;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Sales;
using Ambev.DeveloperEvaluation.ORM.Mapping;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Sales;

public sealed class SaleQueries(DefaultContext context) : ISaleQueries
{
    public IQueryable<SaleListItem> Query() =>
        context.Sales.AsNoTracking().Select(Projection());

    public IQueryable<SaleListItem> QueryByCustomer(Guid customerId) =>
        context.Sales.AsNoTracking()
            .Where(sale => sale.Customer.Id == customerId)
            .Select(Projection());

    private Expression<Func<Sale, SaleListItem>> Projection() => sale => new SaleListItem
    {
        Id = sale.Id,
        SaleNumber = sale.Number.Value,
        Date = sale.SoldAt,
        Status = sale.Status,
        CustomerId = sale.Customer.Id,
        CustomerName = sale.Customer.Name,
        BranchId = sale.Branch.Id,
        BranchName = sale.Branch.Name,
        Total = sale.Total.Amount,
        Items = context.SaleItems
            .Where(item => EF.Property<Guid>(item, SaleConfiguration.ItemForeignKey) == sale.Id)
            .Select(item => new SaleLineItem
            {
                Id = item.Id,
                ProductId = item.Product.Id,
                Title = item.Product.Title,
                Quantity = item.Quantity.Value,
                UnitPrice = item.UnitPrice.Amount,
                Discount = item.Totals.Discount.Amount,
                Total = item.Totals.Net.Amount,
                Status = item.Status
            })
            .ToList()
    };
}
