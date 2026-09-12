using System.Linq.Expressions;
using Ambev.DeveloperEvaluation.Application.Carts.Common;
using Ambev.DeveloperEvaluation.Application.Ports;
using Ambev.DeveloperEvaluation.Domain.Carts;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Carts;

public sealed class CartQueries(DefaultContext context) : ICartQueries
{
    private static readonly Expression<Func<Cart, CartListItem>> Projection = cart => new CartListItem
    {
        Id = cart.Id,
        UserId = cart.CustomerId,
        Date = cart.CreatedAt,
        Status = cart.Status,
        Items = cart.Items
            .Select(item => new CartLineItem
            {
                ProductId = item.Product.Id,
                Title = item.Product.Title,
                Quantity = item.Quantity.Value
            })
            .ToList()
    };

    public IQueryable<CartListItem> Query() =>
        context.Carts.AsNoTracking().Select(Projection);

    public IQueryable<CartListItem> QueryByCustomer(Guid customerId) =>
        context.Carts.AsNoTracking()
            .Where(cart => cart.CustomerId == customerId)
            .Select(Projection);
}
