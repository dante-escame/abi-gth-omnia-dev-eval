using System.Linq.Expressions;
using Ambev.DeveloperEvaluation.Application.Carts.Common;
using Ambev.DeveloperEvaluation.Application.Ports;
using MongoDB.Driver;

namespace Ambev.DeveloperEvaluation.Persistence.Mongo.Carts;

public sealed class MongoCartQueries(CartContext context) : ICartQueries
{
    private static readonly Expression<Func<CartDocument, CartListItem>> Projection = document => new CartListItem
    {
        Id = document.Id,
        UserId = document.UserId,
        Date = document.CreatedAt,
        Status = document.Status,
        Items = document.Products
            .Select(line => new CartLineItem
            {
                ProductId = line.ProductId,
                Title = line.Title,
                Quantity = line.Quantity
            })
            .ToList()
    };

    public IQueryable<CartListItem> Query() =>
        context.Carts.AsQueryable().Select(Projection);

    public IQueryable<CartListItem> QueryByCustomer(Guid customerId) =>
        context.Carts.AsQueryable()
            .Where(document => document.UserId == customerId)
            .Select(Projection);
}
