using Ambev.DeveloperEvaluation.Application;
using Ambev.DeveloperEvaluation.Application.Events;
using Ambev.DeveloperEvaluation.Application.Clock;
using Ambev.DeveloperEvaluation.Application.Common.Lists;
using Ambev.DeveloperEvaluation.Application.Ports;
using Ambev.DeveloperEvaluation.Common.Security;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Sales.Services;
using Ambev.DeveloperEvaluation.ORM.Repositories;
using Ambev.DeveloperEvaluation.ORM;
using Ambev.DeveloperEvaluation.ORM.Clock;
using Ambev.DeveloperEvaluation.ORM.Lists;
using Ambev.DeveloperEvaluation.ORM.Outbox;
using MediatR;
using NSubstitute;
using Rebus.Bus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Outbox;

public sealed class OutboxFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder()
        .WithImage("postgres:13")
        .Build();

    public ServiceProvider Services { get; private set; } = null!;

    public RecordingLoggerProvider Logs { get; } = new();

    public async Task InitializeAsync()
    {
        await _database.StartAsync();

        var services = new ServiceCollection();

        services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Information);
            logging.AddProvider(Logs);
        });

        services.AddDbContext<DefaultContext>(options =>
            options.UseNpgsql(_database.GetConnectionString())
                .AddInterceptors(new OutboxInterceptor()));

        services.AddAutoMapper(typeof(ApplicationLayer).Assembly);
        services.AddMediatR(cfg =>
        {
            cfg.TypeEvaluator = type => type != typeof(IdempotentDomainEventHandler<>);
            cfg.RegisterServicesFromAssembly(typeof(ApplicationLayer).Assembly);
        });
        services.Decorate(typeof(INotificationHandler<>), typeof(IdempotentDomainEventHandler<>));

        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton(Substitute.For<IJwtTokenGenerator>());
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOutboxConsumerTracker, OutboxConsumerTracker>();
        services.Configure<OutboxOptions>(options => { });
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<IPagedQueryExecutor, PagedQueryExecutor>();
        services.AddSingleton(Substitute.For<IDocumentPagedQueryExecutor>());
        services.AddSingleton(Substitute.For<IProductRepository>());
        services.AddSingleton(Substitute.For<IProductQueries>());
        services.AddSingleton(Substitute.For<IProductReader>());
        services.AddSingleton(Substitute.For<IProductEventReplay>());
        services.AddSingleton(Substitute.For<ICartRepository>());
        services.AddSingleton(Substitute.For<ICartQueries>());
        services.AddSingleton(Substitute.For<ISaleRepository>());
        services.AddSingleton(Substitute.For<ISaleQueries>());
        services.AddSingleton(Substitute.For<ISaleNumberGenerator>());
        services.AddSingleton(Substitute.For<IBranchDirectory>());
        services.AddSingleton(Substitute.For<IUnitOfWork>());
        services.AddSingleton(Substitute.For<IBus>());
        services.AddSingleton<IDiscountPolicy, TieredDiscountPolicy>();
        services.AddSingleton<ProcessOutboxJob>();

        Services = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        });

        using var scope = Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<DefaultContext>().Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        if (Services != null!)
            await Services.DisposeAsync();

        await _database.DisposeAsync();
    }

    public async Task ResetAsync()
    {
        Logs.Clear();

        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
        await context.Database.ExecuteSqlRawAsync(
            @"TRUNCATE ""Users"", outbox_messages, outbox_message_consumers");
    }
}
