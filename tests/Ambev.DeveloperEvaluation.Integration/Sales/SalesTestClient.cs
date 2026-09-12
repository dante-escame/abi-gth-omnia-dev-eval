using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace Ambev.DeveloperEvaluation.Integration.Sales;

public static class SalesTestClient
{
    public const string Downtown = "3f2a6c10-5f1e-4b8a-9c2d-0a1b2c3d4e01";

    public const string Riverside = "3f2a6c10-5f1e-4b8a-9c2d-0a1b2c3d4e02";

    public static async Task<Guid> CreateProductAsync(this HttpClient client, string title, decimal price)
    {
        var response = await client.PostAsJsonAsync("/api/products", new
        {
            title,
            price,
            description = $"{title} for sale",
            category = "gear",
            image = "https://example.test/product.png",
            rating = new { rate = 4.1m, count = 12 }
        });

        response.EnsureSuccessStatusCode();

        return await response.IdAsync();
    }

    public static async Task<Guid> CreateCartAsync(
        this HttpClient client,
        params (Guid ProductId, int Quantity)[] lines)
    {
        var response = await client.PostAsJsonAsync("/api/carts", new
        {
            products = lines.Select(line => new { productId = line.ProductId, quantity = line.Quantity }).ToArray()
        });

        response.EnsureSuccessStatusCode();

        return await response.IdAsync();
    }

    public static Task<HttpResponseMessage> PostSaleAsync(this HttpClient client, Guid cartId, string branchId) =>
        client.PostAsJsonAsync("/api/sales", new { cartId, branchId = Guid.Parse(branchId) });

    public static async Task<JsonElement> CreateSaleAsync(this HttpClient client, Guid cartId)
    {
        var response = await client.PostSaleAsync(cartId, Downtown);
        response.EnsureSuccessStatusCode();

        return await response.BodyAsync();
    }

    public static Task<HttpResponseMessage> PutSaleAsync(
        this HttpClient client,
        Guid saleId,
        params (Guid ProductId, int Quantity)[] lines) =>
        client.PutAsJsonAsync($"/api/sales/{saleId}", new
        {
            items = lines.Select(line => new { productId = line.ProductId, quantity = line.Quantity }).ToArray()
        });

    public static async Task<JsonElement> BodyAsync(this HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<JsonElement>();

    public static async Task<Guid> IdAsync(this HttpResponseMessage response) =>
        (await response.BodyAsync()).Id();

    public static Guid Id(this JsonElement element) => element.GuidValue("id");

    public static Guid GuidValue(this JsonElement element, string property) =>
        Guid.Parse(element.GetProperty(property).GetString()!);

    public static string Text(this JsonElement element, string property) =>
        element.GetProperty(property).GetString()!;

    public static decimal Number(this JsonElement element, string property) =>
        element.GetProperty(property).GetDecimal();

    public static IReadOnlyList<JsonElement> Items(this JsonElement sale) =>
        sale.GetProperty("items").EnumerateArray().ToList();

    public static JsonElement ItemOf(this JsonElement sale, Guid productId) =>
        sale.Items().Single(item => item.GuidValue("productId") == productId);

    public static async Task ShouldBeErrorAsync(this HttpResponseMessage response, string expectedType)
    {
        var body = await response.BodyAsync();

        body.Text("type").Should().Be(expectedType);
        body.Text("error").Should().NotBeNullOrWhiteSpace();
        body.Text("detail").Should().NotBeNullOrWhiteSpace();
    }
}
