using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ambev.DeveloperEvaluation.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Users;

public class UsersEndpointsTests(UsersApiFactory factory) : IClassFixture<UsersApiFactory>, IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();

    public Task InitializeAsync() => factory.ResetAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Creating_A_User_Returns_201_With_The_Documented_Body()
    {
        // Arrange
        var body = UsersTestClient.NewUserBody();

        // Act
        var response = await _client.PostAsJsonAsync("/api/users", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await response.Content.ReadFromJsonAsync<JsonElement>();
        Guid.TryParse(created.GetProperty("id").GetString(), out _).Should().BeTrue();
        created.GetProperty("name").GetProperty("firstname").GetString().Should().NotBeEmpty();
        created.GetProperty("address").GetProperty("geolocation").GetProperty("long").GetString().Should().Be("-46.6");
        created.GetProperty("password").GetString().Should().StartWith("$2");
        created.GetProperty("status").GetString().Should().Be("Active");
        created.GetProperty("role").GetString().Should().Be("Customer");
    }

    [Fact]
    public async Task Creating_A_User_With_An_Elevated_Role_And_No_Admin_Caller_Returns_400()
    {
        // Arrange
        var body = UsersTestClient.NewUserBody(role: UserRole.Admin);

        // Act
        var response = await _client.PostAsJsonAsync("/api/users", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await ShouldBeErrorBody(response, "Users.RoleNotAllowed");
    }

    [Fact]
    public async Task Creating_A_User_With_An_Email_That_Is_Taken_Returns_409()
    {
        // Arrange
        await _client.CreateUserAsync(UsersTestClient.NewUserBody(email: "duplicate@example.com"));

        // Act
        var response = await _client.PostAsJsonAsync("/api/users", UsersTestClient.NewUserBody(email: "duplicate@example.com"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        await ShouldBeErrorBody(response, "Users.DuplicateEmail");
    }

    [Fact]
    public async Task Creating_A_User_From_An_Invalid_Body_Returns_400_With_The_Error_Body()
    {
        // Arrange
        var body = UsersTestClient.NewUserBody(email: "not-an-email");

        // Act
        var response = await _client.PostAsJsonAsync("/api/users", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await ShouldBeErrorBody(response, "ValidationError");
    }

    [Fact]
    public async Task Reading_A_User_That_Does_Not_Exist_Returns_404_With_The_Error_Body()
    {
        // Act
        var response = await _client.GetAsync($"/api/users/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await ShouldBeErrorBody(response, "Users.NotFound");
    }

    [Fact]
    public async Task Listing_Users_Without_A_Token_Returns_401()
    {
        // Act
        var response = await _client.GetAsync("/api/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Listing_Users_Pages_Orders_And_Reports_The_Documented_Totals()
    {
        // Arrange
        for (int index = 0; index < 15; index++)
            await _client.CreateUserAsync(UsersTestClient.NewUserBody(
                email: $"user{index:D2}@example.com",
                username: $"user{index:D2}"));

        await _client.AuthenticateAsync("user00@example.com");

        // Act
        var response = await _client.GetAsync("/api/users?_page=2&_size=10&_order=username asc");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("totalItems").GetInt32().Should().Be(15);
        body.GetProperty("totalPages").GetInt32().Should().Be(2);
        body.GetProperty("currentPage").GetInt32().Should().Be(2);

        var usernames = body.GetProperty("data").EnumerateArray()
            .Select(item => item.GetProperty("username").GetString())
            .ToList();

        usernames.Should().HaveCount(5);
        usernames.Should().BeInAscendingOrder();
        usernames[0].Should().Be("user10");
    }

    [Fact]
    public async Task Listing_Users_Filters_With_A_Partial_Match_And_A_Range()
    {
        // Arrange
        await _client.CreateUserAsync(UsersTestClient.NewUserBody(email: "alpha@example.com", username: "alpha"));
        await _client.CreateUserAsync(UsersTestClient.NewUserBody(email: "alpine@example.com", username: "alpine"));
        await _client.CreateUserAsync(UsersTestClient.NewUserBody(email: "beta@example.com", username: "beta"));

        await _client.AuthenticateAsync("alpha@example.com");

        // Act
        var response = await _client.GetAsync("/api/users?username=alp*");
        var contains = await _client.GetAsync("/api/users?email=*example.com");
        var exact = await _client.GetAsync("/api/users?role=Customer&username=beta");

        // Assert
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("totalItems").GetInt32().Should().Be(2);

        var containsBody = await contains.Content.ReadFromJsonAsync<JsonElement>();
        containsBody.GetProperty("totalItems").GetInt32().Should().Be(3);

        var exactBody = await exact.Content.ReadFromJsonAsync<JsonElement>();
        exactBody.GetProperty("totalItems").GetInt32().Should().Be(1);
    }

    [Fact]
    public async Task Updating_A_User_Replaces_The_Mutable_Fields_And_Keeps_The_Password()
    {
        // Arrange
        var created = await _client.CreateUserAsync(UsersTestClient.NewUserBody(email: "editor@example.com", username: "editor"));
        string? id = created.GetProperty("id").GetString();
        string? originalHash = created.GetProperty("password").GetString();

        await _client.AuthenticateAsync("editor@example.com");

        // Act
        var response = await _client.PutAsJsonAsync($"/api/users/{id}", UsersTestClient.NewUserBody(
            email: "renamed@example.com",
            username: "renamed",
            password: null));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var updated = await response.Content.ReadFromJsonAsync<JsonElement>();
        updated.GetProperty("email").GetString().Should().Be("renamed@example.com");
        updated.GetProperty("username").GetString().Should().Be("renamed");
        updated.GetProperty("password").GetString().Should().Be(originalHash);
    }

    [Fact]
    public async Task Updating_A_User_With_A_New_Password_Rehashes_It()
    {
        // Arrange
        var created = await _client.CreateUserAsync(UsersTestClient.NewUserBody(email: "rotate@example.com", username: "rotate"));
        string? id = created.GetProperty("id").GetString();
        string? originalHash = created.GetProperty("password").GetString();

        await _client.AuthenticateAsync("rotate@example.com");

        // Act
        var response = await _client.PutAsJsonAsync($"/api/users/{id}", UsersTestClient.NewUserBody(
            email: "rotate@example.com",
            username: "rotate",
            password: "N3wPassw0rd@1"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var updated = await response.Content.ReadFromJsonAsync<JsonElement>();
        updated.GetProperty("password").GetString().Should().NotBe(originalHash);

        await _client.AuthenticateAsync("rotate@example.com", "N3wPassw0rd@1");
    }

    [Fact]
    public async Task Updating_A_User_That_Does_Not_Exist_Returns_404()
    {
        // Arrange
        await _client.CreateUserAsync(UsersTestClient.NewUserBody(email: "caller@example.com", username: "caller"));
        await _client.AuthenticateAsync("caller@example.com");

        // Act
        var response = await _client.PutAsJsonAsync($"/api/users/{Guid.NewGuid()}", UsersTestClient.NewUserBody());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await ShouldBeErrorBody(response, "Users.NotFound");
    }

    [Fact]
    public async Task Updating_A_User_To_An_Email_That_Is_Taken_Returns_409()
    {
        // Arrange
        await _client.CreateUserAsync(UsersTestClient.NewUserBody(email: "taken@example.com", username: "taken"));
        var mover = await _client.CreateUserAsync(UsersTestClient.NewUserBody(email: "mover@example.com", username: "mover"));

        await _client.AuthenticateAsync("mover@example.com");

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/users/{mover.GetProperty("id").GetString()}",
            UsersTestClient.NewUserBody(email: "taken@example.com", username: "mover"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        await ShouldBeErrorBody(response, "Users.DuplicateEmail");
    }

    [Fact]
    public async Task Promoting_A_User_To_Admin_Without_An_Admin_Caller_Returns_403()
    {
        // Arrange
        var target = await _client.CreateUserAsync(UsersTestClient.NewUserBody(email: "target@example.com", username: "target"));

        await _client.AuthenticateAsync("target@example.com");

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/users/{target.GetProperty("id").GetString()}",
            UsersTestClient.NewUserBody(email: "target@example.com", username: "target", role: UserRole.Admin));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        await ShouldBeErrorBody(response, "Users.RoleChangeForbidden");
    }

    private static async Task ShouldBeErrorBody(HttpResponseMessage response, string expectedType)
    {
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        body.GetProperty("type").GetString().Should().Be(expectedType);
        body.GetProperty("error").GetString().Should().NotBeNullOrWhiteSpace();
        body.GetProperty("detail").GetString().Should().NotBeNullOrWhiteSpace();
    }
}
