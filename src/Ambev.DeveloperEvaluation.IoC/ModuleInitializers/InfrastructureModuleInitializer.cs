using Ambev.DeveloperEvaluation.Application.Clock;
using Ambev.DeveloperEvaluation.Application.Common.Lists;
using Ambev.DeveloperEvaluation.Application.Ports;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.ORM;
using Ambev.DeveloperEvaluation.ORM.Clock;
using Ambev.DeveloperEvaluation.ORM.Lists;
using Ambev.DeveloperEvaluation.ORM.Outbox;
using Ambev.DeveloperEvaluation.ORM.Repositories;
using Ambev.DeveloperEvaluation.Persistence.Mongo;
using Ambev.DeveloperEvaluation.Persistence.Mongo.Lists;
using Ambev.DeveloperEvaluation.Persistence.Mongo.Products;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Ambev.DeveloperEvaluation.IoC.ModuleInitializers;

public class InfrastructureModuleInitializer : IModuleInitializer
{
    public void Initialize(WebApplicationBuilder builder)
    {
        builder.Services.Configure<OutboxOptions>(builder.Configuration.GetSection("Outbox"));
        builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        builder.Services.AddSingleton<OutboxInterceptor>();
        builder.Services.AddScoped<DbContext>(provider => provider.GetRequiredService<DefaultContext>());
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddSingleton<IPagedQueryExecutor, PagedQueryExecutor>();
        builder.Services.AddScoped<IOutboxConsumerTracker, OutboxConsumerTracker>();
        builder.Services.AddSingleton<ProcessOutboxJob>();
        builder.Services.AddHostedService(provider => provider.GetRequiredService<ProcessOutboxJob>());

        MongoSerializationConventions.Register();
        builder.Services.Configure<MongoOptions>(builder.Configuration.GetSection("Mongo"));
        builder.Services.AddSingleton<IMongoClient>(provider =>
            new MongoClient(provider.GetRequiredService<IOptions<MongoOptions>>().Value.ConnectionString));
        builder.Services.AddSingleton(provider =>
            provider.GetRequiredService<IMongoClient>()
                .GetDatabase(provider.GetRequiredService<IOptions<MongoOptions>>().Value.Database));
        builder.Services.AddSingleton<CatalogContext>();
        builder.Services.AddSingleton<IDocumentPagedQueryExecutor, DocumentPagedQueryExecutor>();
        builder.Services.AddScoped<IProductRepository, MongoProductRepository>();
        builder.Services.AddScoped<IProductQueries, MongoProductQueries>();
        builder.Services.AddHostedService<ProductIndexInitializer>();
        builder.Services.AddHealthChecks().AddCheck<MongoHealthCheck>(
            "MongoDB",
            failureStatus: HealthStatus.Unhealthy,
            tags: ["readiness"],
            timeout: TimeSpan.FromSeconds(5));
    }
}