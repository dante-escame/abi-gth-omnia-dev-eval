using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ambev.DeveloperEvaluation.Integration.Common;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Users;

public class LoginRateLimitTests(LoginRateLimitApiFactory factory) : IClassFixture<LoginRateLimitApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [ContainerFact]
    public async Task Login_Attempts_Beyond_The_Window_Limit_Return_429()
    {
        // Arrange
        var credentials = new { email = "throttled@example.com", password = "Passw0rd@1" };

        for (var attempt = 0; attempt < LoginRateLimitApiFactory.PermitLimit; attempt++)
        {
            var permitted = await _client.PostAsJsonAsync("/api/auth", credentials);
            permitted.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth", credentials);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        response.Headers.RetryAfter.Should().NotBeNull();

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("type").GetString().Should().Be("TooManyRequests");
        body.GetProperty("error").GetString().Should().NotBeNullOrWhiteSpace();
        body.GetProperty("detail").GetString().Should().NotBeNullOrWhiteSpace();
    }

    [ContainerFact]
    public async Task Endpoints_Outside_The_Login_Policy_Are_Not_Throttled()
    {
        // Act
        var statuses = new List<HttpStatusCode>();

        for (var attempt = 0; attempt < LoginRateLimitApiFactory.PermitLimit + 5; attempt++)
        {
            var response = await _client.GetAsync("/api/users");
            statuses.Add(response.StatusCode);
        }

        // Assert
        statuses.Should().AllBeEquivalentTo(HttpStatusCode.Unauthorized);
    }
}
