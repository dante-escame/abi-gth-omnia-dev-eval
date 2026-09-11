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

    [Fact(DisplayName = "A minimum date keeps only the carts created on or after it")]
    public async Task Given_MinDate_When_Listed_Then_FiltersInTheStore()
    {
        await SeedAsync();

        var page = await ListAsync($"_minDate={Boundary(-15)}");

        page.TotalItems.Should().Be(2);
        page.Data.Select(cart => cart.Date).Should().OnlyContain(date => date > Anchor.AddDays(-15));
    }

    [Fact(DisplayName = "A maximum date keeps only the carts created on or before it")]
    public async Task Given_MaxDate_When_Listed_Then_FiltersInTheStore()
    {
        await SeedAsync();

        var page = await ListAsync($"_maxDate={Boundary(-15)}");

        page.TotalItems.Should().Be(1);
        page.Data.Single().Date.Should().BeCloseTo(Anchor.AddDays(-20), TimeSpan.FromMilliseconds(2));
    }

    [Fact(DisplayName = "A date range combines both limits keeping the middle of the cart")]
    public async Task Given_DateRange_When_Listed_Then_KeepsTheMiddleCart()
    {
        await SeedAsync();

        var page = await ListAsync($"_minDate={Boundary(-15)}&_maxDate={Boundary(-5)}");

        page.TotalItems.Should().Be(1);
        page.Data.Single().Date.Should().BeCloseTo(Anchor.AddDays(-10), TimeSpan.FromMilliseconds(2));
    }

    [Fact(DisplayName = "An DateTime in ISO format is compared in UTC, not shifted into the local zone")]
    public async Task Given_UtcBoundary_When_Listed_Then_ComparesInUtc()
    {
        await StoreAsync(Cart.Restore(Guid.NewGuid(), _owner, CartStatus.Active, [Line("Helmet", 1)], Anchor, null));
        await StoreAsync(Cart.Restore(Guid.NewGuid(), _owner, CartStatus.Active, [Line("Gloves", 1)], Anchor.AddHours(2), null));

        var page = await ListAsync("_minDate=2026-06-01T13:00:00Z");

        page.TotalItems.Should().Be(1);
        page.Data.Single().Date.Should().Be(Anchor.AddHours(2));
    }

    [Fact(DisplayName = "Ordering by date descending sorts newest first")]
    public async Task Given_DescendingOrder_When_Listed_Then_SortsNewestFirst()
    {
        await SeedAsync();

        var page = await ListAsync("_order=date desc");

        page.Data.Select(cart => cart.Date).Should().BeInDescendingOrder();
        page.Data.First().Date.Should().BeCloseTo(Anchor, TimeSpan.FromMilliseconds(2));
    }

    [Fact(DisplayName = "An exact userId filter keeps only that customer carts")]
    public async Task Given_UserIdFilter_When_Listed_Then_KeepsOneCustomer()
    {
        await SeedAsync();

        var page = await ListAsync($"userId={_stranger}");

        page.TotalItems.Should().Be(1);
        page.Data.Single().UserId.Should().Be(_stranger);
    }

    [Fact(DisplayName = "An exact status filter runs as a plain string comparison")]
    public async Task Given_StatusFilter_When_Listed_Then_KeepsCheckedOut()
    {
        await SeedAsync();
        await StoreAsync(Cart.Restore(
            Guid.NewGuid(),
            _owner,
            CartStatus.CheckedOut,
            [Line("Helmet", 1)],
            Anchor.AddDays(-1),
            null));

        var page = await ListAsync($"status={nameof(CartStatus.CheckedOut)}");

        page.TotalItems.Should().Be(1);
    }

    [Fact(DisplayName = "The embedded line array materializes through the projection")]
    public async Task Given_NestedLines_When_Listed_Then_ProjectsThem()
    {
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

        var page = await ListAsync(string.Empty);

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
