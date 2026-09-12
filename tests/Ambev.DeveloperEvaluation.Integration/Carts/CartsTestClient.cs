using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using FluentAssertions;

namespace Ambev.DeveloperEvaluation.Integration.Carts;

public static class CartsTestClient
{
    private static readonly Faker Faker = new();

    public static object CartBody(params (Guid ProductId, int Quantity)[] lines) => new
    {
        userId = Guid.NewGuid(),
        date = DateTime.UtcNow,
        products = lines.Select(line => new { productId = line.ProductId, quantity = line.Quantity }).ToArray()
    };

    public static object ProductBody(string title) => new
    {
        title,
        price = Faker.Random.Decimal(1m, 500m),
        description = Faker.Commerce.ProductDescription(),
        category = Faker.Commerce.Department(),
        image = "https://example.test/product.png",
        rating = new { rate = 4.1m, count = 12 }
    };

    public static async Task<JsonElement> CreateCartAsync(
        this HttpClient client,
        params (Guid ProductId, int Quantity)[] lines)
    {
        var response = await client.PostAsJsonAsync("/api/carts", CartBody(lines));
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    public static async Task<Guid> CreateProductAsync(this HttpClient client, string title)
    {
        var response = await client.PostAsJsonAsync("/api/products", ProductBody(title));
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        return Guid.Parse(body.GetProperty("id").GetString()!);
    }

    public static async Task RenameProductAsync(this HttpClient client, Guid id, string title)
    {
        var response = await client.PutAsJsonAsync($"/api/products/{id}", ProductBody(title));
        response.EnsureSuccessStatusCode();
    }

    public static async Task ShouldBeErrorBodyAsync(this HttpResponseMessage response, string expectedType)
    {
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        body.GetProperty("type").GetString().Should().Be(expectedType);
        body.GetProperty("error").GetString().Should().NotBeNullOrWhiteSpace();
        body.GetProperty("detail").GetString().Should().NotBeNullOrWhiteSpace();
    }
}
