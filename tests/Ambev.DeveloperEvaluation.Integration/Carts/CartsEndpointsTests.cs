using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ambev.DeveloperEvaluation.Domain.Carts;
using Ambev.DeveloperEvaluation.Domain.Carts.ValueObjects;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Integration.Common;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Carts;

[Collection(SalesContextCollection.Name)]
public class CartsEndpointsTests(SalesContextApiFactory factory) : IAsyncLifetime
{
    public Task InitializeAsync() => factory.ResetAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [ContainerFact]
    public async Task Every_Cart_Endpoint_Refuses_A_Caller_Without_A_Token()
    {
        // Arrange
        var client = factory.CreateClient();
        var id = Guid.NewGuid();

        // Act
        var list = await client.GetAsync("/api/carts");
        var read = await client.GetAsync($"/api/carts/{id}");
        var created = await client.PostAsJsonAsync("/api/carts", CartsTestClient.CartBody((id, 1)));
        var replaced = await client.PutAsJsonAsync($"/api/carts/{id}", CartsTestClient.CartBody((id, 1)));
        var deleted = await client.DeleteAsync($"/api/carts/{id}");

        // Assert
        list.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        read.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        created.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        replaced.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        deleted.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [ContainerFact]
    public async Task Creating_A_Cart_Returns_201_With_A_Location_Header_And_The_Token_Owner()
    {
        // Arrange
        var (customer, customerId) = await factory.SignInAsync(UserRole.Customer);
        var productId = Guid.NewGuid();

        // Act
        var response = await customer.PostAsJsonAsync("/api/carts", CartsTestClient.CartBody((productId, 4)));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        string? id = body.GetProperty("id").GetString();

        Guid.TryParse(id, out _).Should().BeTrue();
        body.GetProperty("userId").GetString().Should().Be(customerId.ToString());
        response.Headers.Location!.ToString().Should().EndWith(id);

        var line = body.GetProperty("products").EnumerateArray().Single();
        line.GetProperty("productId").GetString().Should().Be(productId.ToString());
        line.GetProperty("quantity").GetInt32().Should().Be(4);
    }

    [ContainerFact]
    public async Task Creating_A_Cart_With_An_Empty_Product_List_Returns_400_With_The_Error_Body()
    {
        // Arrange
        var (customer, _) = await factory.SignInAsync(UserRole.Customer);

        // Act
        var response = await customer.PostAsJsonAsync("/api/carts", CartsTestClient.CartBody());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await response.ShouldBeErrorBodyAsync("ValidationError");
    }

    [ContainerFact]
    public async Task Replacing_The_Lines_Of_A_Checked_Out_Cart_Returns_409()
    {
        // Arrange
        var (customer, customerId) = await factory.SignInAsync(UserRole.Customer);
        var cart = await SeedAsync(customerId, CartStatus.CheckedOut);

        // Act
        var response = await customer.PutAsJsonAsync(
            $"/api/carts/{cart.Id}",
            CartsTestClient.CartBody((Guid.NewGuid(), 1)));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        await response.ShouldBeErrorBodyAsync("Carts.AlreadyCheckedOut");
    }

    [ContainerFact]
    public async Task Deleting_A_Checked_Out_Cart_Returns_409()
    {
        // Arrange
        var (customer, customerId) = await factory.SignInAsync(UserRole.Customer);
        var cart = await SeedAsync(customerId, CartStatus.CheckedOut);

        // Act
        var response = await customer.DeleteAsync($"/api/carts/{cart.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        await response.ShouldBeErrorBodyAsync("Carts.AlreadyCheckedOut");
    }

    [ContainerFact]
    public async Task A_Cart_Belonging_To_Someone_Else_Looks_Like_It_Does_Not_Exist()
    {
        // Arrange
        var (_, ownerId) = await factory.SignInAsync(UserRole.Customer);
        var (stranger, _) = await factory.SignInAsync(UserRole.Customer);
        var cart = await SeedAsync(ownerId, CartStatus.Active);

        // Act
        var read = await stranger.GetAsync($"/api/carts/{cart.Id}");
        var replaced = await stranger.PutAsJsonAsync(
            $"/api/carts/{cart.Id}",
            CartsTestClient.CartBody((Guid.NewGuid(), 1)));
        var deleted = await stranger.DeleteAsync($"/api/carts/{cart.Id}");

        // Assert
        read.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await read.ShouldBeErrorBodyAsync("Carts.NotFound");
        replaced.StatusCode.Should().Be(HttpStatusCode.NotFound);
        deleted.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [ContainerFact]
    public async Task A_Customer_Listing_Carts_Sees_Only_Their_Own_Totals_Included()
    {
        // Arrange
        var (customer, customerId) = await factory.SignInAsync(UserRole.Customer);
        var (_, strangerId) = await factory.SignInAsync(UserRole.Customer);

        await SeedAsync(customerId, CartStatus.Active);
        await SeedAsync(strangerId, CartStatus.Active);
        await SeedAsync(strangerId, CartStatus.Active);

        // Act
        var response = await customer.GetAsync("/api/carts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        body.GetProperty("totalItems").GetInt32().Should().Be(1);
        body.GetProperty("currentPage").GetInt32().Should().Be(1);
        body.GetProperty("totalPages").GetInt32().Should().Be(1);
        body.GetProperty("data").EnumerateArray().Single()
            .GetProperty("userId").GetString().Should().Be(customerId.ToString());
    }

    [ContainerFact]
    public async Task A_Manager_Is_Not_Scoped_To_A_Single_Customer()
    {
        // Arrange
        var (_, firstId) = await factory.SignInAsync(UserRole.Customer);
        var (_, secondId) = await factory.SignInAsync(UserRole.Customer);
        var (manager, _) = await factory.SignInAsync(UserRole.Manager);

        await SeedAsync(firstId, CartStatus.Active);
        await SeedAsync(secondId, CartStatus.Active);

        // Act
        var response = await manager.GetAsync("/api/carts");

        // Assert
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("totalItems").GetInt32().Should().Be(2);
    }

    [ContainerFact]
    public async Task Deleting_A_Cart_The_Caller_Owns_Removes_It_For_Good()
    {
        // Arrange
        var (customer, customerId) = await factory.SignInAsync(UserRole.Customer);
        var cart = await SeedAsync(customerId, CartStatus.Active);

        // Act
        var deleted = await customer.DeleteAsync($"/api/carts/{cart.Id}");

        // Assert
        deleted.StatusCode.Should().Be(HttpStatusCode.NoContent);
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
