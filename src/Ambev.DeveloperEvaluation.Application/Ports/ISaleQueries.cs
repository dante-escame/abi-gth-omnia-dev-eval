using Ambev.DeveloperEvaluation.Application.Sales.Common;

namespace Ambev.DeveloperEvaluation.Application.Ports;

public interface ISaleQueries
{
    IQueryable<SaleListItem> Query();

    IQueryable<SaleListItem> QueryByCustomer(Guid customerId);
}
