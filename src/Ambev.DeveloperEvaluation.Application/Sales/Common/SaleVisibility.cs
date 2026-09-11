using Ambev.DeveloperEvaluation.Application.Common;
using Ambev.DeveloperEvaluation.Domain.Sales;

namespace Ambev.DeveloperEvaluation.Application.Sales.Common;

public static class SaleVisibility
{
    public static bool IsVisibleTo(Sale sale, CallerContext caller) =>
        !caller.IsScoped || sale.Customer.Id == caller.UserId;
}
