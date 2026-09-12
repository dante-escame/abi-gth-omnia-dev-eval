using Ambev.DeveloperEvaluation.Application.Common.Results;
using Ambev.DeveloperEvaluation.Application.Users.Common;
using Ambev.DeveloperEvaluation.Common.Security;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Users.ValueObjects;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Users.UpdateUser;

public sealed class UpdateUserHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    : IRequestHandler<UpdateUserCommand, Result<UserResult>>
{
    public async Task<Result<UserResult>> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(command.Id, cancellationToken);
        if (user is null)
            return Result.Failure<UserResult>(UserErrors.NotFound(command.Id));

        var privilegeCheck = CheckPrivileges(user, command);
        if (privilegeCheck.IsFailure)
            return Result.Failure<UserResult>(privilegeCheck.Error!);

        var conflict = await FindConflictAsync(user, command, cancellationToken);
        if (conflict is not null)
            return Result.Failure<UserResult>(conflict);

        user.ChangeProfile(
            new Username(command.Username),
            new Email(command.Email),
            new Phone(command.Phone),
            new PersonName(command.Name.FirstName, command.Name.LastName),
            new Address(
                command.Address.City,
                command.Address.Street,
                command.Address.Number,
                command.Address.ZipCode,
                new Geolocation(command.Address.Geolocation.Lat, command.Address.Geolocation.Long)));

        if (!string.IsNullOrWhiteSpace(command.Password))
            user.ChangePassword(new PasswordHash(passwordHasher.HashPassword(command.Password)));

        if (user.Role != command.Role)
            user.ChangeRole(command.Role);

        if (user.Status != command.Status)
            user.ChangeStatus(command.Status);

        await userRepository.UpdateAsync(user, cancellationToken);

        return Result.Success(UserResult.From(user));
    }

    private static Result CheckPrivileges(User user, UpdateUserCommand command)
    {
        var promotingToAdmin = command.Role == UserRole.Admin && user.Role != UserRole.Admin;
        var changesRoleOrStatus = user.Role != command.Role || user.Status != command.Status;

        if (promotingToAdmin && command.CallerRole != UserRole.Admin)
            return Result.Failure(UserErrors.RoleChangeForbidden);

        if (changesRoleOrStatus && command.CallerRole is not (UserRole.Admin or UserRole.Manager))
            return Result.Failure(UserErrors.RoleOrStatusChangeForbidden);

        return Result.Success();
    }

    private async Task<Error?> FindConflictAsync(User user, UpdateUserCommand command, CancellationToken cancellationToken)
    {
        if (!string.Equals(user.Email.Value, command.Email, StringComparison.OrdinalIgnoreCase))
        {
            var byEmail = await userRepository.GetByEmailAsync(command.Email, cancellationToken);
            if (byEmail is not null && byEmail.Id != user.Id)
                return UserErrors.DuplicateEmail(command.Email);
        }

        if (!string.Equals(user.Username.Value, command.Username, StringComparison.Ordinal))
        {
            var byUsername = await userRepository.GetByUsernameAsync(command.Username, cancellationToken);
            if (byUsername is not null && byUsername.Id != user.Id)
                return UserErrors.DuplicateUsername(command.Username);
        }

        return null;
    }
}
