using Ambev.DeveloperEvaluation.ORM;
using Ambev.DeveloperEvaluation.WebApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Users;

public sealed class UsersApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder()
        .WithImage("postgres:13")
        .Build();

    public async Task InitializeAsync()
    {
        await _database.StartAsync();

        using var scope = Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<DefaultContext>().Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _database.DisposeAsync();
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
                ["Outbox:IntervalInSeconds"] = "3600"
            }));
    }
}
