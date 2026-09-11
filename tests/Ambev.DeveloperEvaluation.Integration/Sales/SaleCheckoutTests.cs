using System.Net;
using System.Net.Http.Json;
using Ambev.DeveloperEvaluation.Domain.Carts;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Sales.Events;
using Ambev.DeveloperEvaluation.Integration.Common;
using Ambev.DeveloperEvaluation.ORM;
using Ambev.DeveloperEvaluation.ORM.Outbox;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Sales;

[Collection(SalesContextCollection.Name)]
public sealed class SaleCheckoutTests(SalesContextApiFactory factory) : IAsyncLifetime
{
    private HttpClient _catalog = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        if (!ContainerRuntime.IsAvailable)
            return;

        await factory.ResetAsync();
        (_catalog, _) = await factory.SignInAsync(UserRole.Admin);
        (_client, _) = await factory.SignInAsync(UserRole.Customer);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [ContainerFact]
    public async Task The_Cart_Is_Already_Checked_Out_On_The_Very_Next_Read()
    {
        // Arrange
        var product = await _catalog.CreateProductAsync("Helmet", 10.00m);
        var cartId = await _client.CreateCartAsync((product, 1));

        // Act
        var sale = await _client.CreateSaleAsync(cartId);

        // Assert
        var cart = await LoadCartAsync(cartId);

        cart!.Status.Should().Be(CartStatus.CheckedOut);
        cart.SaleId.Should().Be(sale.Id());
        cart.CheckedOutAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [ContainerFact]
    public async Task A_Checked_Out_Cart_Refuses_Every_Further_Change()
    {
        // Arrange
        var product = await _catalog.CreateProductAsync("Helmet", 10.00m);
        var cartId = await _client.CreateCartAsync((product, 1));
        await _client.CreateSaleAsync(cartId);

        // Act
        var update = await _client.PutAsJsonAsync(
            $"/api/carts/{cartId}",
            new { products = new[] { new { productId = product, quantity = 2 } } });
        var delete = await _client.DeleteAsync($"/api/carts/{cartId}");

        // Assert
        update.StatusCode.Should().Be(HttpStatusCode.Conflict);
        delete.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [ContainerFact]
    public async Task A_Rejected_Sale_Writes_Nothing_And_Leaves_The_Cart_Active()
    {
        // Arrange
        var product = await _catalog.CreateProductAsync("Helmet", 10.00m);
        var cartId = await _client.CreateCartAsync((product, 21));

        // Act
        var response = await _client.PostSaleAsync(cartId, SalesTestClient.Downtown);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();

        (await context.Sales.IgnoreQueryFilters().CountAsync()).Should().Be(0);
        (await SaleEvents(context).CountAsync()).Should().Be(0);

        var cart = await LoadCartAsync(cartId);
        cart!.Status.Should().Be(CartStatus.Active);
        cart.SaleId.Should().BeNull();
    }

    [ContainerFact]
    public async Task Cancelling_Or_Deleting_A_Sale_Never_Reopens_Its_Cart()
    {
        // Arrange
        var product = await _catalog.CreateProductAsync("Helmet", 10.00m);
        var cartId = await _client.CreateCartAsync((product, 1));
        var sale = await _client.CreateSaleAsync(cartId);

        // Act
        (await _client.PatchAsync($"/api/sales/{sale.Id()}/cancel", null)).EnsureSuccessStatusCode();
        (await _client.DeleteAsync($"/api/sales/{sale.Id()}")).EnsureSuccessStatusCode();

        // Assert
        var cart = await LoadCartAsync(cartId);
        cart!.Status.Should().Be(CartStatus.CheckedOut);
        cart.SaleId.Should().Be(sale.Id());
    }

    [ContainerFact]
    public async Task Creating_A_Sale_Leaves_An_Unprocessed_Row_That_One_Cycle_Publishes()
    {
        // Arrange
        var product = await _catalog.CreateProductAsync("Helmet", 10.00m);
        var cartId = await _client.CreateCartAsync((product, 1));

        // Act
        await _client.CreateSaleAsync(cartId);

        // Assert
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
            var pending = await SaleEvents(context).SingleAsync();

            pending.Type.Should().EndWith(nameof(SaleCreatedDomainEvent));
            pending.ProcessedOnUtc.Should().BeNull();
        }

        await factory.RunSaleOutboxCycleAsync();

        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
            var published = await SaleEvents(context).SingleAsync();

            published.ProcessedOnUtc.Should().NotBeNull();
            published.Error.Should().BeNull();
        }

        (await factory.RunSaleOutboxCycleAsync()).Should().Be(0);
    }

    [ContainerFact]
    public async Task Cancelling_The_Last_Item_Puts_Both_Events_In_The_Outbox()
    {
        // Arrange
        var product = await _catalog.CreateProductAsync("Helmet", 10.00m);
        var sale = await _client.CreateSaleAsync(await _client.CreateCartAsync((product, 1)));
        var item = sale.ItemOf(product);

        // Act
        (await _client.PatchAsync($"/api/sales/{sale.Id()}/items/{item.Id()}/cancel", null))
            .EnsureSuccessStatusCode();

        // Assert
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();

        var types = await SaleEvents(context).Select(message => message.Type).ToListAsync();

        types.Should().HaveCount(3);
        types.Should().ContainSingle(type => type.EndsWith(nameof(ItemCancelledDomainEvent)));
        types.Should().ContainSingle(type => type.EndsWith(nameof(SaleCancelledDomainEvent)));
    }

    private async Task<Cart?> LoadCartAsync(Guid cartId)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();

        return await context.Carts.AsNoTracking().SingleOrDefaultAsync(cart => cart.Id == cartId);
    }

    private static IQueryable<OutboxMessage> SaleEvents(DefaultContext context) =>
        context.OutboxMessages.AsNoTracking()
            .Where(message => message.Type.StartsWith("Ambev.DeveloperEvaluation.Domain.Sales."));
}
