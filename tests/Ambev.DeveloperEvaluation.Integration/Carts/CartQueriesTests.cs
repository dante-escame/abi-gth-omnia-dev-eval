using Ambev.DeveloperEvaluation.Application.Carts.Common;
using Ambev.DeveloperEvaluation.Application.Common.Lists;
using Ambev.DeveloperEvaluation.Application.Ports;
using Ambev.DeveloperEvaluation.Domain.Carts;
using Ambev.DeveloperEvaluation.Domain.Carts.ValueObjects;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Carts;

[Collection(CartsCollection.Name)]
public class CartQueriesTests(CartsApiFactory factory) : IAsyncLifetime
{
    private static readonly DateTime Anchor = new(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly Guid _owner = Guid.NewGuid();

    private readonly Guid _stranger = Guid.NewGuid();

    public Task InitializeAsync() => factory.ResetAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task A_Minimum_Date_Keeps_Only_The_Carts_Created_On_Or_After_It()
    {
        // Arrange
        await SeedAsync();

        // Act
        var page = await ListAsync($"_minDate={Boundary(-15)}");

        // Assert
        page.TotalItems.Should().Be(2);
        page.Data.Select(cart => cart.Date).Should().OnlyContain(date => date > Anchor.AddDays(-15));
    }

    [Fact]
    public async Task A_Maximum_Date_Keeps_Only_The_Carts_Created_On_Or_Before_It()
    {
        // Arrange
        await SeedAsync();

        // Act
        var page = await ListAsync($"_maxDate={Boundary(-15)}");

        // Assert
        page.TotalItems.Should().Be(1);
        page.Data.Single().Date.Should().BeCloseTo(Anchor.AddDays(-20), TimeSpan.FromMilliseconds(2));
    }

    [Fact]
    public async Task A_Date_Range_Combines_Both_Limits_And_Keeps_The_Cart_In_The_Middle()
    {
        // Arrange
        await SeedAsync();

        // Act
        var page = await ListAsync($"_minDate={Boundary(-15)}&_maxDate={Boundary(-5)}");

        // Assert
        page.TotalItems.Should().Be(1);
        page.Data.Single().Date.Should().BeCloseTo(Anchor.AddDays(-10), TimeSpan.FromMilliseconds(2));
    }

    [Fact]
    public async Task A_Date_In_Iso_Format_Is_Compared_In_Utc_And_Not_Shifted_Into_The_Local_Zone()
    {
        // Arrange
        await StoreAsync(Cart.Restore(Guid.NewGuid(), _owner, CartStatus.Active, [Line("Helmet", 1)], Anchor, null));
        await StoreAsync(Cart.Restore(Guid.NewGuid(), _owner, CartStatus.Active, [Line("Gloves", 1)], Anchor.AddHours(2), null));

        // Act
        var page = await ListAsync("_minDate=2026-06-01T13:00:00Z");

        // Assert
        page.TotalItems.Should().Be(1);
        page.Data.Single().Date.Should().Be(Anchor.AddHours(2));
    }

    [Fact]
    public async Task Ordering_By_Date_Descending_Sorts_The_Newest_Cart_First()
    {
        // Arrange
        await SeedAsync();

        // Act
        var page = await ListAsync("_order=date desc");

        // Assert
        page.Data.Select(cart => cart.Date).Should().BeInDescendingOrder();
        page.Data.First().Date.Should().BeCloseTo(Anchor, TimeSpan.FromMilliseconds(2));
    }

    [Fact]
    public async Task An_Exact_User_Id_Filter_Keeps_Only_That_Customers_Carts()
    {
        // Arrange
        await SeedAsync();

        // Act
        var page = await ListAsync($"userId={_stranger}");

        // Assert
        page.TotalItems.Should().Be(1);
        page.Data.Single().UserId.Should().Be(_stranger);
    }

    [Fact]
    public async Task An_Exact_Status_Filter_Runs_As_A_Plain_String_Comparison()
    {
        // Arrange
        await SeedAsync();
        await StoreAsync(Cart.Restore(
            Guid.NewGuid(),
            _owner,
            CartStatus.CheckedOut,
            [Line("Helmet", 1)],
            Anchor.AddDays(-1),
            null));

        // Act
        var page = await ListAsync($"status={nameof(CartStatus.CheckedOut)}");

        // Assert
        page.TotalItems.Should().Be(1);
    }

    [Fact]
    public async Task The_Embedded_Line_Array_Materializes_Through_The_Projection()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var titleless = Guid.NewGuid();

        await StoreAsync(Cart.Restore(
            Guid.NewGuid(),
            _owner,
            CartStatus.Active,
            [
                new CartItem(new ProductRef(productId, "Mountain Bike"), new Quantity(4)),
                new CartItem(new ProductRef(titleless), new Quantity(2))
            ],
            Anchor,
            null));

        // Act
        var page = await ListAsync(string.Empty);

        // Assert
        var cart = page.Data.Should().ContainSingle().Subject;

        cart.Products.Should().HaveCount(2);
        cart.Products.Single(line => line.ProductId == productId).Title.Should().Be("Mountain Bike");
        cart.Products.Single(line => line.ProductId == productId).Quantity.Should().Be(4);
        cart.Products.Single(line => line.ProductId == titleless).Title.Should().BeNull();
    }

    private static string Boundary(int days) => Anchor.AddDays(days).ToString("yyyy-MM-ddTHH:mm:ssZ");

    private static CartItem Line(string title, int quantity) =>
        new(new ProductRef(Guid.NewGuid(), title), new Quantity(quantity));

    private async Task SeedAsync()
    {
        await StoreAsync(Cart.Restore(Guid.NewGuid(), _owner, CartStatus.Active, [Line("Helmet", 1)], Anchor.AddDays(-20), null));
        await StoreAsync(Cart.Restore(Guid.NewGuid(), _owner, CartStatus.Active, [Line("Mountain Bike", 2)], Anchor.AddDays(-10), null));
        await StoreAsync(Cart.Restore(Guid.NewGuid(), _stranger, CartStatus.Active, [Line("Gloves", 3)], Anchor, null));
    }

    private async Task StoreAsync(Cart cart)
    {
        using var scope = factory.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<ICartRepository>().CreateAsync(cart);
    }

    private async Task<PagedResult<CartResult>> ListAsync(string queryString)
    {
        using var scope = factory.Services.CreateScope();
        var queries = scope.ServiceProvider.GetRequiredService<ICartQueries>();
        var executor = scope.ServiceProvider.GetRequiredService<IDocumentPagedQueryExecutor>();

        var list = ListQueryParser.Parse(Parse(queryString));

        var source = queries.Query()
            .ApplyFilters(list, CartListFields.Map)
            .ApplyOrdering(list, CartListFields.Map);

        var page = await executor.ToPagedResultAsync(source, list.Page, list.Size);

        return page.Map(CartResult.From);
    }

    private static Dictionary<string, string?> Parse(string queryString) => queryString
        .Split('&', StringSplitOptions.RemoveEmptyEntries)
        .Select(pair => pair.Split('=', 2))
        .ToDictionary(pair => pair[0], pair => (string?)pair[1]);
}
