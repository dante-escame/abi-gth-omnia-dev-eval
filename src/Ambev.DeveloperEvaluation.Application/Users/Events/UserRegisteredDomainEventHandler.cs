using Ambev.DeveloperEvaluation.Domain.Users.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Users.Events;

public sealed class UserRegisteredDomainEventHandler(ILogger<UserRegisteredDomainEventHandler> logger)
    : INotificationHandler<UserRegisteredDomainEvent>
{
    public Task Handle(UserRegisteredDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "User registered {UserId} {Username} {Email} {Role} {OccurredOnUtc}",
            notification.UserId,
            notification.Username,
            notification.Email,
            notification.Role,
            notification.OccurredOnUtc);

        return Task.CompletedTask;
    }
}
