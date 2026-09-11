using Ambev.DeveloperEvaluation.ORM;
using Ambev.DeveloperEvaluation.WebApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MongoDb;
using Testcontainers.PostgreSql;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Users;

public class UsersApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder()
        .WithImage("postgres:13")
        .Build();

    private readonly MongoDbContainer _catalog = new MongoDbBuilder()
        .WithImage("mongo:8.0.16")
        .Build();

    protected virtual int LoginPermitLimit => UsersTestClient.UnthrottledLoginPermitLimit;

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_database.StartAsync(), _catalog.StartAsync());

        using var scope = Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<DefaultContext>().Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _database.DisposeAsync();
        await _catalog.DisposeAsync();
    }

    public async Task ResetAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
        await context.Database.ExecuteSqlRawAsync(@"TRUNCATE ""Users"", outbox_messages, outbox_message_consumers");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _database.GetConnectionString(),
                ["Mongo:ConnectionString"] = _catalog.GetConnectionString(),
                ["Mongo:Database"] = "developer_evaluation",
                ["Outbox:IntervalInSeconds"] = "3600",
                ["RateLimiting:Login:PermitLimit"] = LoginPermitLimit.ToString()
            }));
    }
}
