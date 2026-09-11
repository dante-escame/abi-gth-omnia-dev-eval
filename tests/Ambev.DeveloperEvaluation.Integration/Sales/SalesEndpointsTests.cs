using System.Net;
using System.Net.Http.Json;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Integration.Common;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Sales;

[Collection(SalesContextCollection.Name)]
public sealed class SalesEndpointsTests(SalesContextApiFactory factory) : IAsyncLifetime
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
    public async Task Every_Sale_Endpoint_Refuses_A_Caller_Without_A_Token()
    {
        // Arrange
        var anonymous = factory.CreateClient();
        var id = Guid.NewGuid();

        // Act
        var responses = new[]
        {
            await anonymous.GetAsync("/api/sales"),
            await anonymous.GetAsync($"/api/sales/{id}"),
            await anonymous.PostAsJsonAsync("/api/sales", new { cartId = id, branchId = id }),
            await anonymous.PutAsJsonAsync($"/api/sales/{id}", new { items = Array.Empty<object>() }),
            await anonymous.PatchAsync($"/api/sales/{id}/cancel", null),
            await anonymous.DeleteAsync($"/api/sales/{id}"),
            await anonymous.PatchAsync($"/api/sales/{id}/items/{id}/cancel", null)
        };

        // Assert
        responses.Should().OnlyContain(response => response.StatusCode == HttpStatusCode.Unauthorized);
    }

    [ContainerFact]
    public async Task Creating_A_Sale_Returns_201_With_A_Location_Header_And_The_Priced_Cart()
    {
        // Arrange
        var product = await _catalog.CreateProductAsync("Helmet", 10.00m);
        var cart = await _client.CreateCartAsync((product, 2));

        // Act
        var response = await _client.PostSaleAsync(cart, SalesTestClient.Downtown);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var sale = await response.BodyAsync();

        response.Headers.Location!.ToString().Should().EndWith($"/api/Sales/{sale.Id()}");
        sale.Text("saleNumber").Should().MatchRegex(@"^SALE-\d{6,}$");
        sale.Text("status").Should().Be(nameof(SaleStatus.Active));
        sale.Text("branchName").Should().Be("Downtown Store");
        sale.Number("total").Should().Be(20.00m);

        var item = sale.ItemOf(product);
        item.Text("title").Should().Be("Helmet");
        item.Number("unitPrice").Should().Be(10.00m);
        item.Number("discount").Should().Be(0m);
        item.Number("total").Should().Be(20.00m);
    }

    [ContainerFact]
    public async Task Creating_A_Sale_Applies_Ten_And_Twenty_Percent_On_The_Lines_That_Earn_Them()
    {
        // Arrange
        var cheap = await _catalog.CreateProductAsync("Helmet", 10.00m);
        var pricey = await _catalog.CreateProductAsync("Gloves", 5.00m);
        var cart = await _client.CreateCartAsync((cheap, 4), (pricey, 10));

        // Act
        var sale = await _client.CreateSaleAsync(cart);

        // Assert
        sale.ItemOf(cheap).Number("discount").Should().Be(4.00m);
        sale.ItemOf(cheap).Number("total").Should().Be(36.00m);
        sale.ItemOf(pricey).Number("discount").Should().Be(10.00m);
        sale.ItemOf(pricey).Number("total").Should().Be(40.00m);

        sale.Number("total").Should().Be(76.00m);
        sale.Number("total").Should().Be(sale.Items().Sum(item => item.Number("total")));
    }

    [ContainerFact]
    public async Task Creating_A_Sale_From_A_Line_Above_Twenty_Units_Returns_422()
    {
        // Arrange
        var product = await _catalog.CreateProductAsync("Helmet", 10.00m);
        var cart = await _client.CreateCartAsync((product, 21));

        // Act
        var response = await _client.PostSaleAsync(cart, SalesTestClient.Downtown);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        await response.ShouldBeErrorAsync("Sales.QuantityExceeded");
    }

    [ContainerFact]
    public async Task Creating_A_Sale_At_A_Branch_Nobody_Seeded_Returns_422()
    {
        // Arrange
        var product = await _catalog.CreateProductAsync("Helmet", 10.00m);
        var cart = await _client.CreateCartAsync((product, 1));

        // Act
        var response = await _client.PostSaleAsync(cart, Guid.NewGuid().ToString());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        await response.ShouldBeErrorAsync("Sales.UnknownBranch");
    }

    [ContainerFact]
    public async Task Creating_A_Second_Sale_From_The_Same_Cart_Returns_409()
    {
        // Arrange
        var product = await _catalog.CreateProductAsync("Helmet", 10.00m);
        var cart = await _client.CreateCartAsync((product, 1));
        await _client.CreateSaleAsync(cart);

        // Act
        var response = await _client.PostSaleAsync(cart, SalesTestClient.Downtown);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        await response.ShouldBeErrorAsync("Sales.CartAlreadyCheckedOut");
    }

    [ContainerFact]
    public async Task A_Cart_Belonging_To_Someone_Else_Cannot_Be_Sold()
    {
        // Arrange
        var product = await _catalog.CreateProductAsync("Helmet", 10.00m);
        var (stranger, _) = await factory.SignInAsync(UserRole.Customer);
        var cart = await stranger.CreateCartAsync((product, 1));

        // Act
        var response = await _client.PostSaleAsync(cart, SalesTestClient.Downtown);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        await response.ShouldBeErrorAsync("Sales.UnknownCart");
    }

    [ContainerFact]
    public async Task Reading_A_Sale_By_Id_Returns_Every_Item_Including_The_Cancelled_Ones()
    {
        // Arrange
        var cheap = await _catalog.CreateProductAsync("Helmet", 10.00m);
        var pricey = await _catalog.CreateProductAsync("Gloves", 5.00m);
        var cart = await _client.CreateCartAsync((cheap, 1), (pricey, 1));
        var sale = await _client.CreateSaleAsync(cart);
        var cancelled = sale.ItemOf(cheap).Id();

        (await _client.PatchAsync($"/api/sales/{sale.Id()}/items/{cancelled}/cancel", null))
            .EnsureSuccessStatusCode();

        // Act
        var response = await _client.GetAsync($"/api/sales/{sale.Id()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.BodyAsync();
        body.Items().Should().HaveCount(2);
        body.ItemOf(cheap).Text("status").Should().Be(nameof(SaleItemStatus.Cancelled));
        body.ItemOf(pricey).Text("status").Should().Be(nameof(SaleItemStatus.Active));
    }

    [ContainerFact]
    public async Task Reading_A_Sale_That_Does_Not_Exist_Returns_404()
    {
        // Act
        var response = await _client.GetAsync($"/api/sales/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await response.ShouldBeErrorAsync("Sales.NotFound");
    }

    [ContainerFact]
    public async Task Listing_Sales_Returns_The_Documented_Paging_Envelope()
    {
        // Arrange
        var product = await _catalog.CreateProductAsync("Helmet", 10.00m);
        await _client.CreateSaleAsync(await _client.CreateCartAsync((product, 1)));

        // Act
        var body = await (await _client.GetAsync("/api/sales")).BodyAsync();

        // Assert
        body.GetProperty("totalItems").GetInt32().Should().Be(1);
        body.GetProperty("currentPage").GetInt32().Should().Be(1);
        body.GetProperty("totalPages").GetInt32().Should().Be(1);
        body.GetProperty("data").EnumerateArray().Single().Items().Should().HaveCount(1);
    }

    [ContainerFact]
    public async Task Listing_Sales_Orders_By_Total_Descending()
    {
        // Arrange
        var product = await _catalog.CreateProductAsync("Helmet", 10.00m);
        await _client.CreateSaleAsync(await _client.CreateCartAsync((product, 1)));
        await _client.CreateSaleAsync(await _client.CreateCartAsync((product, 3)));
        await _client.CreateSaleAsync(await _client.CreateCartAsync((product, 2)));

        // Act
        var body = await (await _client.GetAsync("/api/sales?_order=total desc")).BodyAsync();

        // Assert
        body.GetProperty("data").EnumerateArray()
            .Select(sale => sale.Number("total"))
            .Should().ContainInOrder(30.00m, 20.00m, 10.00m);
    }

    [ContainerFact]
    public async Task Listing_Sales_Filters_By_Status_And_Keeps_Soft_Deleted_Sales_Out()
    {
        // Arrange
        var product = await _catalog.CreateProductAsync("Helmet", 10.00m);
        var kept = await _client.CreateSaleAsync(await _client.CreateCartAsync((product, 1)));
        var cancelled = await _client.CreateSaleAsync(await _client.CreateCartAsync((product, 2)));
        var deleted = await _client.CreateSaleAsync(await _client.CreateCartAsync((product, 3)));

        (await _client.PatchAsync($"/api/sales/{cancelled.Id()}/cancel", null)).EnsureSuccessStatusCode();
        (await _client.DeleteAsync($"/api/sales/{deleted.Id()}")).EnsureSuccessStatusCode();

        // Act
        var active = await (await _client.GetAsync("/api/sales?status=Active")).BodyAsync();
        var all = await (await _client.GetAsync("/api/sales")).BodyAsync();

        // Assert
        active.GetProperty("data").EnumerateArray().Select(sale => sale.Id())
            .Should().Equal(kept.Id());
        all.GetProperty("data").EnumerateArray().Select(sale => sale.Id())
            .Should().BeEquivalentTo(new[] { kept.Id(), cancelled.Id() });
    }

    [ContainerFact]
    public async Task Listing_Sales_Filters_On_A_Real_Sold_At_Range_And_A_Partial_Sale_Number()
    {
        // Arrange
        var product = await _catalog.CreateProductAsync("Helmet", 10.00m);
        var sale = await _client.CreateSaleAsync(await _client.CreateCartAsync((product, 1)));

        var from = DateTime.UtcNow.AddMinutes(-5).ToString("O");
        var to = DateTime.UtcNow.AddMinutes(5).ToString("O");

        // Act
        var inside = await (await _client.GetAsync(
            $"/api/sales?_minDate={from}&_maxDate={to}&saleNumber=SALE*")).BodyAsync();
        var outside = await (await _client.GetAsync(
            $"/api/sales?_minDate={DateTime.UtcNow.AddDays(1):O}")).BodyAsync();

        // Assert
        inside.GetProperty("data").EnumerateArray().Select(row => row.Id()).Should().Equal(sale.Id());
        outside.GetProperty("totalItems").GetInt32().Should().Be(0);
    }

    [ContainerFact]
    public async Task A_Sale_Belonging_To_Someone_Else_Looks_Like_It_Does_Not_Exist()
    {
        // Arrange
        var product = await _catalog.CreateProductAsync("Helmet", 10.00m);
        var sale = await _client.CreateSaleAsync(await _client.CreateCartAsync((product, 1)));
        var (stranger, _) = await factory.SignInAsync(UserRole.Customer);

        // Act
        var byId = await stranger.GetAsync($"/api/sales/{sale.Id()}");
        var list = await (await stranger.GetAsync("/api/sales")).BodyAsync();

        // Assert
        byId.StatusCode.Should().Be(HttpStatusCode.NotFound);
        list.GetProperty("totalItems").GetInt32().Should().Be(0);
    }

    [ContainerFact]
    public async Task Updating_The_Quantities_Recalculates_Every_Total()
    {
        // Arrange
        var product = await _catalog.CreateProductAsync("Helmet", 10.00m);
        var sale = await _client.CreateSaleAsync(await _client.CreateCartAsync((product, 1)));

        // Act
        var response = await _client.PutSaleAsync(sale.Id(), (product, 4));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.BodyAsync();
        body.ItemOf(product).Number("quantity").Should().Be(4);
        body.ItemOf(product).Number("unitPrice").Should().Be(10.00m);
        body.ItemOf(product).Number("discount").Should().Be(4.00m);
        body.Number("total").Should().Be(36.00m);
    }

    [ContainerFact]
    public async Task Updating_A_Sale_With_A_Product_It_Never_Sold_Returns_422()
    {
        // Arrange
        var product = await _catalog.CreateProductAsync("Helmet", 10.00m);
        var other = await _catalog.CreateProductAsync("Gloves", 5.00m);
        var sale = await _client.CreateSaleAsync(await _client.CreateCartAsync((product, 1)));

        // Act
        var response = await _client.PutSaleAsync(sale.Id(), (other, 1));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        await response.ShouldBeErrorAsync("Sales.UnknownItem");
    }

    [ContainerFact]
    public async Task Updating_A_Sale_With_An_Empty_Item_List_Returns_422()
    {
        // Arrange
        var product = await _catalog.CreateProductAsync("Helmet", 10.00m);
        var sale = await _client.CreateSaleAsync(await _client.CreateCartAsync((product, 1)));

        // Act
        var response = await _client.PutSaleAsync(sale.Id());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        await response.ShouldBeErrorAsync("Sales.EmptySale");
    }

    [ContainerFact]
    public async Task Updating_A_Cancelled_Sale_Returns_409()
    {
        // Arrange
        var product = await _catalog.CreateProductAsync("Helmet", 10.00m);
        var sale = await _client.CreateSaleAsync(await _client.CreateCartAsync((product, 1)));
        (await _client.PatchAsync($"/api/sales/{sale.Id()}/cancel", null)).EnsureSuccessStatusCode();

        // Act
        var response = await _client.PutSaleAsync(sale.Id(), (product, 2));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        await response.ShouldBeErrorAsync("Sales.AlreadyCancelled");
    }

    [ContainerFact]
    public async Task Cancelling_A_Sale_Keeps_The_Row_And_Is_Idempotent()
    {
        // Arrange
        var product = await _catalog.CreateProductAsync("Helmet", 10.00m);
        var sale = await _client.CreateSaleAsync(await _client.CreateCartAsync((product, 1)));

        // Act
        var first = await _client.PatchAsJsonAsync($"/api/sales/{sale.Id()}/cancel", new { reason = "changed my mind" });
        var second = await _client.PatchAsync($"/api/sales/{sale.Id()}/cancel", null);

        // Assert
        first.StatusCode.Should().Be(HttpStatusCode.OK);
        second.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await second.BodyAsync();
        body.Text("status").Should().Be(nameof(SaleStatus.Cancelled));
        body.Number("total").Should().Be(0m);
        body.Items().Should().OnlyContain(item => item.Text("status") == nameof(SaleItemStatus.Cancelled));

        (await _client.GetAsync($"/api/sales/{sale.Id()}")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [ContainerFact]
    public async Task Deleting_A_Sale_Hides_It_Everywhere_And_Is_Idempotent()
    {
        // Arrange
        var product = await _catalog.CreateProductAsync("Helmet", 10.00m);
        var sale = await _client.CreateSaleAsync(await _client.CreateCartAsync((product, 1)));

        // Act
        var first = await _client.DeleteAsync($"/api/sales/{sale.Id()}");
        var second = await _client.DeleteAsync($"/api/sales/{sale.Id()}");

        // Assert
        first.StatusCode.Should().Be(HttpStatusCode.NoContent);
        second.StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await _client.GetAsync($"/api/sales/{sale.Id()}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await (await _client.GetAsync("/api/sales")).BodyAsync())
            .GetProperty("totalItems").GetInt32().Should().Be(0);
    }

    [ContainerFact]
    public async Task Cancelling_A_Sale_That_Does_Not_Exist_Returns_404_On_Both_Paths()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var cancel = await _client.PatchAsync($"/api/sales/{id}/cancel", null);
        var delete = await _client.DeleteAsync($"/api/sales/{id}");

        // Assert
        cancel.StatusCode.Should().Be(HttpStatusCode.NotFound);
        delete.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [ContainerFact]
    public async Task Cancelling_One_Item_Drops_The_Sale_Total_By_Exactly_That_Net()
    {
        // Arrange
        var cheap = await _catalog.CreateProductAsync("Helmet", 10.00m);
        var pricey = await _catalog.CreateProductAsync("Gloves", 5.00m);
        var sale = await _client.CreateSaleAsync(await _client.CreateCartAsync((cheap, 4), (pricey, 2)));
        var item = sale.ItemOf(cheap);

        // Act
        var response = await _client.PatchAsync($"/api/sales/{sale.Id()}/items/{item.Id()}/cancel", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.BodyAsync();
        body.Number("total").Should().Be(sale.Number("total") - item.Number("total"));
        body.Text("status").Should().Be(nameof(SaleStatus.Active));
        body.ItemOf(cheap).Text("status").Should().Be(nameof(SaleItemStatus.Cancelled));
        body.ItemOf(cheap).Number("total").Should().Be(item.Number("total"));
    }

    [ContainerFact]
    public async Task Cancelling_The_Last_Active_Item_Cancels_The_Whole_Sale()
    {
        // Arrange
        var product = await _catalog.CreateProductAsync("Helmet", 10.00m);
        var sale = await _client.CreateSaleAsync(await _client.CreateCartAsync((product, 1)));
        var item = sale.ItemOf(product);

        // Act
        var response = await _client.PatchAsync($"/api/sales/{sale.Id()}/items/{item.Id()}/cancel", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.BodyAsync();
        body.Text("status").Should().Be(nameof(SaleStatus.Cancelled));
        body.Number("total").Should().Be(0m);
    }

    [ContainerFact]
    public async Task Cancelling_An_Item_Twice_Or_On_A_Cancelled_Sale_Returns_409()
    {
        // Arrange
        var cheap = await _catalog.CreateProductAsync("Helmet", 10.00m);
        var pricey = await _catalog.CreateProductAsync("Gloves", 5.00m);
        var sale = await _client.CreateSaleAsync(await _client.CreateCartAsync((cheap, 1), (pricey, 1)));
        var item = sale.ItemOf(cheap);

        (await _client.PatchAsync($"/api/sales/{sale.Id()}/items/{item.Id()}/cancel", null))
            .EnsureSuccessStatusCode();

        // Act
        var again = await _client.PatchAsync($"/api/sales/{sale.Id()}/items/{item.Id()}/cancel", null);

        (await _client.PatchAsync($"/api/sales/{sale.Id()}/cancel", null)).EnsureSuccessStatusCode();

        var onCancelledSale = await _client.PatchAsync(
            $"/api/sales/{sale.Id()}/items/{sale.ItemOf(pricey).Id()}/cancel",
            null);

        // Assert
        again.StatusCode.Should().Be(HttpStatusCode.Conflict);
        await again.ShouldBeErrorAsync("Sales.ItemAlreadyCancelled");

        onCancelledSale.StatusCode.Should().Be(HttpStatusCode.Conflict);
        await onCancelledSale.ShouldBeErrorAsync("Sales.AlreadyCancelled");
    }

    [ContainerFact]
    public async Task Cancelling_An_Item_That_Is_Not_On_The_Sale_Returns_404()
    {
        // Arrange
        var product = await _catalog.CreateProductAsync("Helmet", 10.00m);
        var sale = await _client.CreateSaleAsync(await _client.CreateCartAsync((product, 1)));

        // Act
        var response = await _client.PatchAsync($"/api/sales/{sale.Id()}/items/{Guid.NewGuid()}/cancel", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await response.ShouldBeErrorAsync("Sales.ItemNotFound");
    }
}
