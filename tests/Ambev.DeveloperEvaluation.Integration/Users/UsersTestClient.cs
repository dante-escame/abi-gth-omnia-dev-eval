using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Bogus;

namespace Ambev.DeveloperEvaluation.Integration.Users;

public static class UsersTestClient
{
    public const int UnthrottledLoginPermitLimit = 10_000;

    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public static object NewUserBody(
        string? email = null,
        string? username = null,
        UserRole role = UserRole.Customer,
        UserStatus status = UserStatus.Active,
        string? password = "Passw0rd@1")
    {
        var faker = new Faker();

        return new
        {
            email = email ?? faker.Internet.Email(),
            username = username ?? faker.Internet.UserName().PadRight(3, 'x'),
            password,
            name = new { firstname = faker.Name.FirstName(), lastname = faker.Name.LastName() },
            address = new
            {
                city = faker.Address.City(),
                street = faker.Address.StreetName(),
                number = faker.Random.Int(1, 9999),
                zipcode = faker.Address.ZipCode(),
                geolocation = new { lat = "-23.5", @long = "-46.6" }
            },
            phone = $"+55{faker.Random.Number(11, 99)}{faker.Random.Number(100000000, 999999999)}",
            status = status.ToString(),
            role = role.ToString()
        };
    }

    public static async Task<JsonElement> CreateUserAsync(this HttpClient client, object body)
    {
        var response = await client.PostAsJsonAsync("/api/users", body);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    public static async Task AuthenticateAsync(this HttpClient client, string email, string password = "Passw0rd@1")
    {
        var response = await client.PostAsJsonAsync("/api/auth", new { email, password });
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var token = body.GetProperty("token").GetString();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
}
