using Ambev.DeveloperEvaluation.Application.Common.Lists;

namespace Ambev.DeveloperEvaluation.Application.Carts.Common;

public static class CartListFields
{
    public static readonly ListFieldMap<CartListItem> Map = new ListFieldMap<CartListItem>()
        .Map("id", cart => cart.Id)
        .Map("userId", cart => cart.UserId)
        .Map("date", cart => cart.Date)
        .Map("status", cart => cart.Status);
}
