using Ambev.DeveloperEvaluation.Common.Security;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Users.ValueObjects;
using Ambev.DeveloperEvaluation.Integration.Users;
using Ambev.DeveloperEvaluation.ORM;
using Ambev.DeveloperEvaluation.Persistence.Mongo;
using Ambev.DeveloperEvaluation.Persistence.Mongo.Outbox;
using Ambev.DeveloperEvaluation.Persistence.Mongo.Products;
using Ambev.DeveloperEvaluation.WebApi;
using Bogus;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using Testcontainers.MongoDb;
using Testcontainers.PostgreSql;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Carts;

public sealed class CartsApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string Password = "Passw0rd@1";

    private static readonly Faker Faker = new();

    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder()
        .WithImage("postgres:13")
        .Build();

    private readonly MongoDbContainer _documents = new MongoDbBuilder()
        .WithImage("mongo:8.0.16")
        .Build();

    public CatalogContext Catalog => Services.GetRequiredService<CatalogContext>();

    public IMongoDatabase Database => Services.GetRequiredService<IMongoDatabase>();

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_database.StartAsync(), _documents.StartAsync());

        using var scope = Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<DefaultContext>().Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _database.DisposeAsync();
        await _documents.DisposeAsync();
    }

    public async Task ResetAsync()
    {
        using (var scope = Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
            await context.Database.ExecuteSqlRawAsync(
                @"TRUNCATE ""Users"", carts, cart_items, outbox_messages, outbox_message_consumers");
        }

        await Catalog.Products.DeleteManyAsync(FilterDefinition<ProductDocument>.Empty);
    }

    public IMongoCollection<BsonDocument> RawCollection(string name) =>
        Database.GetCollection<BsonDocument>(name);

    public Task<int> RunOutboxCycleAsync() =>
        Services.GetRequiredService<ProcessCatalogOutboxJob>().ProcessBatchAsync();

    public async Task<(HttpClient Client, Guid UserId)> SignInAsync(UserRole role)
    {
        var email = $"user{Guid.NewGuid():N}@example.test";
        var userId = await SeedUserAsync(email, role);
        var client = CreateClient();

        await client.AuthenticateAsync(email, Password);

        return (client, userId);
    }

    private async Task<Guid> SeedUserAsync(string email, UserRole role)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var user = User.Register(
            new Username(email[..email.IndexOf('@')]),
            new Email(email),
            new Phone($"+55{Faker.Random.Number(11, 99)}{Faker.Random.Number(100000000, 999999999)}"),
            new PasswordHash(hasher.HashPassword(Password)),
            new PersonName(Faker.Name.FirstName(), Faker.Name.LastName()),
            new Address(
                Faker.Address.City(),
                Faker.Address.StreetName(),
                Faker.Random.Int(1, 9999),
                Faker.Address.ZipCode(),
                new Geolocation("-23.5", "-46.6")),
            role,
            UserStatus.Active);

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return user.Id;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _database.GetConnectionString(),
                ["Mongo:ConnectionString"] = _documents.GetConnectionString(),
                ["Mongo:Database"] = "developer_evaluation",
                ["Outbox:IntervalInSeconds"] = "3600",
                ["CatalogOutbox:IntervalInSeconds"] = "3600",
                ["RateLimiting:Login:PermitLimit"] = UsersTestClient.UnthrottledLoginPermitLimit.ToString()
            }));
    }
}

[CollectionDefinition(CartsCollection.Name)]
public sealed class CartsCollection : ICollectionFixture<CartsApiFactory>
{
    public const string Name = "Carts";
}
