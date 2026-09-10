using Ambev.DeveloperEvaluation.Application.Common.Results;
using Ambev.DeveloperEvaluation.Application.Users.Common;
using Ambev.DeveloperEvaluation.Domain.Enums;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Users.UpdateUser;

public sealed class UpdateUserCommand : IRequest<Result<UserResult>>
{
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string? Password { get; set; }

    public PersonNameInput Name { get; set; } = PersonNameInput.Empty;

    public AddressInput Address { get; set; } = AddressInput.Empty;

    public string Phone { get; set; } = string.Empty;

    public UserStatus Status { get; set; }

    public UserRole Role { get; set; }

    public UserRole? CallerRole { get; set; }
}
