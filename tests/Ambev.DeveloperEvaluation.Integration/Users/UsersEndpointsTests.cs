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

    [Fact(DisplayName = "POST /users creates the user with the documented body")]
    public async Task Post_ValidBody_Creates()
    {
        var body = UsersTestClient.NewUserBody();

        var response = await _client.PostAsJsonAsync("/api/users", body);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await response.Content.ReadFromJsonAsync<JsonElement>();
        Guid.TryParse(created.GetProperty("id").GetString(), out _).Should().BeTrue();
        created.GetProperty("name").GetProperty("firstname").GetString().Should().NotBeEmpty();
        created.GetProperty("address").GetProperty("geolocation").GetProperty("long").GetString().Should().Be("-46.6");
        created.GetProperty("password").GetString().Should().StartWith("$2");
        created.GetProperty("status").GetString().Should().Be("Active");
        created.GetProperty("role").GetString().Should().Be("Customer");
    }

    [Fact(DisplayName = "POST /users rejects an elevated role without an admin caller")]
    public async Task Post_ElevatedRole_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/users", UsersTestClient.NewUserBody(role: UserRole.Admin));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await ShouldBeErrorBody(response, "Users.RoleNotAllowed");
    }

    [Fact(DisplayName = "POST /users returns 409 for a duplicate email")]
    public async Task Post_DuplicateEmail_ReturnsConflict()
    {
        var first = UsersTestClient.NewUserBody(email: "duplicate@example.com");
        await _client.CreateUserAsync(first);

        var response = await _client.PostAsJsonAsync("/api/users", UsersTestClient.NewUserBody(email: "duplicate@example.com"));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        await ShouldBeErrorBody(response, "Users.DuplicateEmail");
    }

    [Fact(DisplayName = "POST /users returns 400 with the error body for invalid input")]
    public async Task Post_InvalidBody_ReturnsValidationError()
    {
        var response = await _client.PostAsJsonAsync("/api/users", UsersTestClient.NewUserBody(email: "not-an-email"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await ShouldBeErrorBody(response, "ValidationError");
    }

    [Fact(DisplayName = "GET /users/{id} returns 404 with the error body for an unknown id")]
    public async Task Get_UnknownId_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/users/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await ShouldBeErrorBody(response, "Users.NotFound");
    }

    [Fact(DisplayName = "GET /users requires a valid JWT")]
    public async Task List_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/users");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "GET /users pages, orders and reports the documented totals")]
    public async Task List_SecondPage_ReturnsFiveRows()
    {
        for (var index = 0; index < 15; index++)
            await _client.CreateUserAsync(UsersTestClient.NewUserBody(
                email: $"user{index:D2}@example.com",
                username: $"user{index:D2}"));

        await _client.AuthenticateAsync("user00@example.com");

        var response = await _client.GetAsync("/api/users?_page=2&_size=10&_order=username asc");
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

    [Fact(DisplayName = "GET /users filters with a partial match and a range")]
    public async Task List_PartialFilter_ReturnsMatches()
    {
        await _client.CreateUserAsync(UsersTestClient.NewUserBody(email: "alpha@example.com", username: "alpha"));
        await _client.CreateUserAsync(UsersTestClient.NewUserBody(email: "alpine@example.com", username: "alpine"));
        await _client.CreateUserAsync(UsersTestClient.NewUserBody(email: "beta@example.com", username: "beta"));

        await _client.AuthenticateAsync("alpha@example.com");

        var response = await _client.GetAsync("/api/users?username=alp*");
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        body.GetProperty("totalItems").GetInt32().Should().Be(2);

        var contains = await _client.GetAsync("/api/users?email=*example.com");
        var containsBody = await contains.Content.ReadFromJsonAsync<JsonElement>();
        containsBody.GetProperty("totalItems").GetInt32().Should().Be(3);

        var exact = await _client.GetAsync("/api/users?role=Customer&username=beta");
        var exactBody = await exact.Content.ReadFromJsonAsync<JsonElement>();
        exactBody.GetProperty("totalItems").GetInt32().Should().Be(1);
    }

    [Fact(DisplayName = "PUT /users/{id} replaces the mutable fields and keeps the password")]
    public async Task Put_ValidBody_Updates()
    {
        var created = await _client.CreateUserAsync(UsersTestClient.NewUserBody(email: "editor@example.com", username: "editor"));
        var id = created.GetProperty("id").GetString();
        var originalHash = created.GetProperty("password").GetString();

        await _client.AuthenticateAsync("editor@example.com");

        var response = await _client.PutAsJsonAsync($"/api/users/{id}", UsersTestClient.NewUserBody(
            email: "renamed@example.com",
            username: "renamed",
            password: null));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var updated = await response.Content.ReadFromJsonAsync<JsonElement>();
        updated.GetProperty("email").GetString().Should().Be("renamed@example.com");
        updated.GetProperty("username").GetString().Should().Be("renamed");
        updated.GetProperty("password").GetString().Should().Be(originalHash);
    }

    [Fact(DisplayName = "PUT /users/{id} re-hashes the password when a new one is supplied")]
    public async Task Put_WithPassword_RehashesPassword()
    {
        var created = await _client.CreateUserAsync(UsersTestClient.NewUserBody(email: "rotate@example.com", username: "rotate"));
        var id = created.GetProperty("id").GetString();
        var originalHash = created.GetProperty("password").GetString();

        await _client.AuthenticateAsync("rotate@example.com");

        var response = await _client.PutAsJsonAsync($"/api/users/{id}", UsersTestClient.NewUserBody(
            email: "rotate@example.com",
            username: "rotate",
            password: "N3wPassw0rd@1"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var updated = await response.Content.ReadFromJsonAsync<JsonElement>();
        updated.GetProperty("password").GetString().Should().NotBe(originalHash);

        await _client.AuthenticateAsync("rotate@example.com", "N3wPassw0rd@1");
    }

    [Fact(DisplayName = "PUT /users/{id} returns 404 for an unknown id")]
    public async Task Put_UnknownId_ReturnsNotFound()
    {
        await _client.CreateUserAsync(UsersTestClient.NewUserBody(email: "caller@example.com", username: "caller"));
        await _client.AuthenticateAsync("caller@example.com");

        var response = await _client.PutAsJsonAsync($"/api/users/{Guid.NewGuid()}", UsersTestClient.NewUserBody());

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await ShouldBeErrorBody(response, "Users.NotFound");
    }

    [Fact(DisplayName = "PUT /users/{id} returns 409 when the new email is taken")]
    public async Task Put_DuplicateEmail_ReturnsConflict()
    {
        await _client.CreateUserAsync(UsersTestClient.NewUserBody(email: "taken@example.com", username: "taken"));
        var mover = await _client.CreateUserAsync(UsersTestClient.NewUserBody(email: "mover@example.com", username: "mover"));

        await _client.AuthenticateAsync("mover@example.com");

        var response = await _client.PutAsJsonAsync(
            $"/api/users/{mover.GetProperty("id").GetString()}",
            UsersTestClient.NewUserBody(email: "taken@example.com", username: "mover"));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        await ShouldBeErrorBody(response, "Users.DuplicateEmail");
    }

    [Fact(DisplayName = "PUT /users/{id} forbids a non-admin promoting a user to Admin")]
    public async Task Put_NonAdminPromoting_ReturnsForbidden()
    {
        var target = await _client.CreateUserAsync(UsersTestClient.NewUserBody(email: "target@example.com", username: "target"));

        await _client.AuthenticateAsync("target@example.com");

        var response = await _client.PutAsJsonAsync(
            $"/api/users/{target.GetProperty("id").GetString()}",
            UsersTestClient.NewUserBody(email: "target@example.com", username: "target", role: UserRole.Admin));

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
