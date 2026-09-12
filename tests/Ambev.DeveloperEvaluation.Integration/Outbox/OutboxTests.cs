using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Users.Events;
using Ambev.DeveloperEvaluation.Domain.Users.ValueObjects;
using Ambev.DeveloperEvaluation.ORM;
using Ambev.DeveloperEvaluation.ORM.Outbox;
using Bogus;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Outbox;

public class OutboxTests(OutboxFixture fixture) : IClassFixture<OutboxFixture>, IAsyncLifetime
{
    private static readonly string BcryptHash = new Faker().Random.Hash(53).ToLowerInvariant();

    public Task InitializeAsync() => fixture.ResetAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Saving_A_User_Writes_One_Outbox_Row_In_The_Same_Transaction()
    {
        // Arrange
        var user = NewUser();

        // Act
        using (var scope = fixture.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
            context.Users.Add(user);
            await context.SaveChangesAsync();

            user.DomainEvents.Should().BeEmpty();
        }

        // Assert
        using (var scope = fixture.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();

            var messages = await context.OutboxMessages.AsNoTracking().ToListAsync();

            var message = messages.Should().ContainSingle().Subject;
            message.Type.Should().Contain(nameof(UserRegisteredDomainEvent));
            message.Payload.Should().Contain(user.Email.Value);
            message.ProcessedOnUtc.Should().BeNull();
            message.Error.Should().BeNull();

            (await context.Users.CountAsync()).Should().Be(1);
        }
    }

    [Fact]
    public async Task A_Failed_Save_Writes_Neither_The_User_Nor_The_Outbox_Row()
    {
        // Arrange
        var first = NewUser();
        await SaveAsync(first);

        var duplicate = NewUser(email: first.Email.Value);

        // Act
        using (var scope = fixture.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
            context.Users.Add(duplicate);

            var save = async () => await context.SaveChangesAsync();
            await save.Should().ThrowAsync<DbUpdateException>();
        }

        // Assert
        using (var scope = fixture.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
            (await context.Users.CountAsync()).Should().Be(1);
            (await context.OutboxMessages.CountAsync()).Should().Be(1);
        }
    }

    [Fact]
    public async Task An_Outbox_Cycle_Publishes_The_Pending_Message_And_Logs_It_Once()
    {
        // Arrange
        var user = NewUser();
        await SaveAsync(user);

        // Act
        int processed = await fixture.Services.GetRequiredService<ProcessOutboxJob>().ProcessBatchAsync();

        // Assert
        processed.Should().Be(1);

        LogLines().Should().ContainSingle()
            .Which.Should().Contain(user.Id.ToString()).And.Contain(user.Username.Value);

        using var scope = fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();

        var message = await context.OutboxMessages.AsNoTracking().SingleAsync();
        message.ProcessedOnUtc.Should().NotBeNull();
        message.Error.Should().BeNull();

        var consumer = await context.OutboxMessageConsumers.AsNoTracking().SingleAsync();
        consumer.OutboxMessageId.Should().Be(message.Id);
        consumer.HandlerName.Should().Be("UserRegisteredDomainEventHandler");
    }

    [Fact]
    public async Task A_Duplicated_Dispatch_Runs_The_Handler_Only_Once()
    {
        // Arrange
        await SaveAsync(NewUser());
        var job = fixture.Services.GetRequiredService<ProcessOutboxJob>();

        // Act
        await job.ProcessBatchAsync();
        await ReopenMessagesAsync();
        await job.ProcessBatchAsync();

        // Assert
        LogLines().Should().ContainSingle();

        using var scope = fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
        (await context.OutboxMessageConsumers.CountAsync()).Should().Be(1);
    }

    private async Task SaveAsync(User user)
    {
        using var scope = fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
        context.Users.Add(user);
        await context.SaveChangesAsync();
    }

    private async Task ReopenMessagesAsync()
    {
        using var scope = fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
        await context.Database.ExecuteSqlRawAsync(@"UPDATE outbox_messages SET ""ProcessedOnUtc"" = NULL");
    }

    private List<string> LogLines() => fixture.Logs.Entries
        .Where(entry => entry.Level == LogLevel.Information && entry.Message.StartsWith("User registered"))
        .Select(entry => entry.Message)
        .ToList();

    private static User NewUser(string? email = null)
    {
        var faker = new Faker();
        string username = faker.Internet.UserName().PadRight(3, 'x');

        return User.Register(
            new Username(username.Length > 50 ? username[..50] : username),
            new Email(email ?? faker.Internet.Email()),
            new Phone($"+55{faker.Random.Number(11, 99)}{faker.Random.Number(100000000, 999999999)}"),
            new PasswordHash($"$2a${faker.Random.Number(10, 12)}${BcryptHash}"),
            new PersonName(faker.Name.FirstName(), faker.Name.LastName()),
            new Address(
                faker.Address.City(),
                faker.Address.StreetName(),
                faker.Random.Int(1, 9999),
                faker.Address.ZipCode(),
                new Geolocation(faker.Address.Latitude().ToString(), faker.Address.Longitude().ToString())),
            UserRole.Customer,
            UserStatus.Active);
    }
}
