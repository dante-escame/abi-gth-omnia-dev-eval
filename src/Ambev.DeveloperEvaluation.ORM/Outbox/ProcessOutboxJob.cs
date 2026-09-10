using System.Collections.Concurrent;
using System.Text.Json;
using Ambev.DeveloperEvaluation.Application.Clock;
using Ambev.DeveloperEvaluation.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ambev.DeveloperEvaluation.ORM.Outbox;

public sealed class ProcessOutboxJob(
    IServiceScopeFactory scopeFactory,
    IDateTimeProvider dateTimeProvider,
    IOptions<OutboxOptions> outboxOptions,
    ILogger<ProcessOutboxJob> logger)
    : BackgroundService
{
    private const string SelectPendingSql = """
        SELECT * FROM outbox_messages
        WHERE "ProcessedOnUtc" IS NULL
        ORDER BY "OccurredOnUtc"
        LIMIT {0}
        FOR UPDATE SKIP LOCKED
        """;

    private static readonly ConcurrentDictionary<string, Type> ResolvedTypes = new();

    private readonly OutboxOptions _options = outboxOptions.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_options.IntervalInSeconds));

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await ProcessBatchAsync(stoppingToken);
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "Outbox cycle failed");
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    public async Task<int> ProcessBatchAsync(CancellationToken cancellationToken = default)
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var messages = await context.OutboxMessages
            .FromSqlRaw(SelectPendingSql, _options.BatchSize)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                await publisher.Publish(Deserialize(message), cancellationToken);
                message.ProcessedOnUtc = dateTimeProvider.UtcNow;
                message.Error = null;
            }
            catch (Exception exception)
            {
                message.Error = exception.ToString();
                logger.LogError(exception, "Outbox message {OutboxMessageId} failed", message.Id);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return messages.Count;
    }

    private static IDomainEvent Deserialize(OutboxMessage message)
    {
        var type = ResolvedTypes.GetOrAdd(
            message.Type,
            name => typeof(IDomainEvent).Assembly.GetType(name)
                ?? Type.GetType(name)
                ?? throw new InvalidOperationException($"Outbox message type {name} could not be resolved."));

        return (IDomainEvent)JsonSerializer.Deserialize(message.Payload, type, OutboxSerializer.Options)!;
    }
}
