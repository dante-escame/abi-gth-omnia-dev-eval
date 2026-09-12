using Ambev.DeveloperEvaluation.Application.Ports;
using Ambev.DeveloperEvaluation.Application.Products.Common;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Ambev.DeveloperEvaluation.Persistence.Mongo.Products;

public sealed class MongoProductQueries(CatalogContext context) : IProductQueries
{
    private const string CategoryField = "category";

    public IQueryable<ProductListItem> Query() =>
        context.Products.AsQueryable().Where(document => document.DeletedAt == null).Select(document => new ProductListItem
        {
            Id = document.Id,
            Title = document.Title,
            Price = document.Price,
            Description = document.Description,
            Category = document.Category,
            Image = document.Image,
            Rate = document.Rate,
            RatingCount = document.RatingCount
        });

    public IQueryable<ProductListItem> QueryByCategory(string category)
    {
        string normalized = category.Trim().ToLowerInvariant();

        return context.Products.AsQueryable()
            .Where(document => document.DeletedAt == null && document.Category.ToLower() == normalized)
            .Select(document => new ProductListItem
            {
                Id = document.Id,
                Title = document.Title,
                Price = document.Price,
                Description = document.Description,
                Category = document.Category,
                Image = document.Image,
                Rate = document.Rate,
                RatingCount = document.RatingCount
            });
    }

    public async Task<IReadOnlyList<string>> ListCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var cursor = await context.Products.DistinctAsync<string>(
            CategoryField,
            Builders<ProductDocument>.Filter.Eq(document => document.DeletedAt, null),
            cancellationToken: cancellationToken);

        var categories = await cursor.ToListAsync(cancellationToken);

        return categories
            .Where(category => !string.IsNullOrWhiteSpace(category))
            .Select(category => category.Trim())
            .GroupBy(category => category.ToLowerInvariant())
            .Select(group => group.OrderBy(category => category, StringComparer.Ordinal).First())
            .OrderBy(category => category, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
