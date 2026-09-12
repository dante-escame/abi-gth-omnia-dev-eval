using Ambev.DeveloperEvaluation.Application.Clock;
using Ambev.DeveloperEvaluation.Application.Common.Lists;
using Ambev.DeveloperEvaluation.Application.Ports;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.ORM;
using Ambev.DeveloperEvaluation.ORM.Clock;
using Ambev.DeveloperEvaluation.ORM.Lists;
using Ambev.DeveloperEvaluation.ORM.Outbox;
using Ambev.DeveloperEvaluation.ORM.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
    }
}