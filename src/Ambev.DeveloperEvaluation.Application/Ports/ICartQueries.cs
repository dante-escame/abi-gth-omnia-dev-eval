using Ambev.DeveloperEvaluation.Application.Carts.Common;

namespace Ambev.DeveloperEvaluation.Application.Ports;

public interface ICartQueries
{
    IQueryable<CartListItem> Query();

    IQueryable<CartListItem> QueryByCustomer(Guid customerId);
}
