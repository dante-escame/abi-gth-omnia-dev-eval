using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ambev.DeveloperEvaluation.Domain.Carts;
using Ambev.DeveloperEvaluation.Domain.Carts.ValueObjects;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Carts;

[Collection(CartsCollection.Name)]
public class CartsEndpointsTests(CartsApiFactory factory) : IAsyncLifetime
{
    public Task InitializeAsync() => factory.ResetAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact(DisplayName = "Every cart endpoint refuses a caller without a token")]
    public async Task Given_NoToken_When_Called_Then_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();
        var id = Guid.NewGuid();

        (await client.GetAsync("/api/carts")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await client.GetAsync($"/api/carts/{id}")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await client.PostAsJsonAsync("/api/carts", CartsTestClient.CartBody((id, 1)))).StatusCode
            .Should().Be(HttpStatusCode.Unauthorized);
        (await client.PutAsJsonAsync($"/api/carts/{id}", CartsTestClient.CartBody((id, 1)))).StatusCode
            .Should().Be(HttpStatusCode.Unauthorized);
        (await client.DeleteAsync($"/api/carts/{id}")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "Creating a cart answers 201 with a location header and the token owner")]
    public async Task Given_ValidBody_When_Posted_Then_ReturnsCreated()
    {
        var (customer, customerId) = await factory.SignInAsync(UserRole.Customer);
        var productId = Guid.NewGuid();

        var response = await customer.PostAsJsonAsync("/api/carts", CartsTestClient.CartBody((productId, 4)));

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var id = body.GetProperty("id").GetString();

        Guid.TryParse(id, out _).Should().BeTrue();
        body.GetProperty("userId").GetString().Should().Be(customerId.ToString());
        response.Headers.Location!.ToString().Should().EndWith(id);

        var line = body.GetProperty("products").EnumerateArray().Single();
        line.GetProperty("productId").GetString().Should().Be(productId.ToString());
        line.GetProperty("quantity").GetInt32().Should().Be(4);
    }

    [Fact(DisplayName = "An empty product list is rejected with the documented error body")]
    public async Task Given_NoLines_When_Posted_Then_ReturnsValidationError()
    {
        var (customer, _) = await factory.SignInAsync(UserRole.Customer);

        var response = await customer.PostAsJsonAsync("/api/carts", CartsTestClient.CartBody());

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await response.ShouldBeErrorBodyAsync("ValidationError");
    }

    [Fact(DisplayName = "A checked out cart refuses a replacement with 409")]
    public async Task Given_CheckedOutCart_When_Updated_Then_ReturnsConflict()
    {
        var (customer, customerId) = await factory.SignInAsync(UserRole.Customer);
        var cart = await SeedAsync(customerId, CartStatus.CheckedOut);

        var response = await customer.PutAsJsonAsync(
            $"/api/carts/{cart.Id}",
            CartsTestClient.CartBody((Guid.NewGuid(), 1)));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        await response.ShouldBeErrorBodyAsync("Carts.AlreadyCheckedOut");
    }

    [Fact(DisplayName = "A checked out cart refuses a delete with 409")]
    public async Task Given_CheckedOutCart_When_Deleted_Then_ReturnsConflict()
    {
        var (customer, customerId) = await factory.SignInAsync(UserRole.Customer);
        var cart = await SeedAsync(customerId, CartStatus.CheckedOut);

        var response = await customer.DeleteAsync($"/api/carts/{cart.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        await response.ShouldBeErrorBodyAsync("Carts.AlreadyCheckedOut");
    }

    [Fact(DisplayName = "Someone else's cart looks like it does not exist")]
    public async Task Given_ForeignCart_When_Called_Then_ReturnsNotFound()
    {
        var (_, ownerId) = await factory.SignInAsync(UserRole.Customer);
        var (stranger, _) = await factory.SignInAsync(UserRole.Customer);
        var cart = await SeedAsync(ownerId, CartStatus.Active);

        var read = await stranger.GetAsync($"/api/carts/{cart.Id}");
        read.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await read.ShouldBeErrorBodyAsync("Carts.NotFound");

        var replaced = await stranger.PutAsJsonAsync(
            $"/api/carts/{cart.Id}",
            CartsTestClient.CartBody((Guid.NewGuid(), 1)));
        replaced.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var deleted = await stranger.DeleteAsync($"/api/carts/{cart.Id}");
        deleted.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "A customer listing carts sees only their own, totals included")]
    public async Task Given_ForeignCarts_When_Listed_Then_CountsOnlyOwn()
    {
        var (customer, customerId) = await factory.SignInAsync(UserRole.Customer);
        var (_, strangerId) = await factory.SignInAsync(UserRole.Customer);

        await SeedAsync(customerId, CartStatus.Active);
        await SeedAsync(strangerId, CartStatus.Active);
        await SeedAsync(strangerId, CartStatus.Active);

        var response = await customer.GetAsync("/api/carts");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        body.GetProperty("totalItems").GetInt32().Should().Be(1);
        body.GetProperty("currentPage").GetInt32().Should().Be(1);
        body.GetProperty("totalPages").GetInt32().Should().Be(1);
        body.GetProperty("data").EnumerateArray().Single()
            .GetProperty("userId").GetString().Should().Be(customerId.ToString());
    }

    [Fact(DisplayName = "A manager is not scoped to a single customer")]
    public async Task Given_Manager_When_Listing_Then_SeesEveryCart()
    {
        var (_, firstId) = await factory.SignInAsync(UserRole.Customer);
        var (_, secondId) = await factory.SignInAsync(UserRole.Customer);
        var (manager, _) = await factory.SignInAsync(UserRole.Manager);

        await SeedAsync(firstId, CartStatus.Active);
        await SeedAsync(secondId, CartStatus.Active);

        var body = await (await manager.GetAsync("/api/carts")).Content.ReadFromJsonAsync<JsonElement>();

        body.GetProperty("totalItems").GetInt32().Should().Be(2);
    }

    [Fact(DisplayName = "A cart the caller owns is deleted for good")]
    public async Task Given_OwnCart_When_Deleted_Then_ReturnsNoContent()
    {
        var (customer, customerId) = await factory.SignInAsync(UserRole.Customer);
        var cart = await SeedAsync(customerId, CartStatus.Active);

        (await customer.DeleteAsync($"/api/carts/{cart.Id}")).StatusCode
            .Should().Be(HttpStatusCode.NoContent);

        (await customer.GetAsync($"/api/carts/{cart.Id}")).StatusCode
            .Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<Cart> SeedAsync(Guid customerId, CartStatus status)
    {
        var cart = Cart.Restore(
            Guid.NewGuid(),
            customerId,
            status,
            [new CartItem(new ProductRef(Guid.NewGuid(), "Helmet"), new Quantity(1))],
            DateTime.UtcNow.AddMinutes(-5),
            null);

        using var scope = factory.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<ICartRepository>().CreateAsync(cart);

        return cart;
    }
}
