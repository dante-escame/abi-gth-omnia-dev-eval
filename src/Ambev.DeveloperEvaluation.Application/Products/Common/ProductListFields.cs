using Ambev.DeveloperEvaluation.Application.Common.Lists;

namespace Ambev.DeveloperEvaluation.Application.Products.Common;

public static class ProductListFields
{
    public static readonly ListFieldMap<ProductListItem> Map = new ListFieldMap<ProductListItem>()
        .Map("id", product => product.Id)
        .Map("title", product => product.Title)
        .Map("price", product => product.Price)
        .Map("description", product => product.Description)
        .Map("category", product => product.Category)
        .Map("image", product => product.Image)
        .Map("rate", product => product.Rate)
        .Map("count", product => product.RatingCount);
}
