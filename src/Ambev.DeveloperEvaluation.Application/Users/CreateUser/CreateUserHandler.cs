using Ambev.DeveloperEvaluation.Application.Common.Results;
using Ambev.DeveloperEvaluation.Application.Users.Common;
using Ambev.DeveloperEvaluation.Common.Security;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Users.ValueObjects;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Users.CreateUser;

public class CreateUserHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    : IRequestHandler<CreateUserCommand, Result<UserResult>>
{
    public async Task<Result<UserResult>> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        if (command.Role != UserRole.Customer && command.CallerRole != UserRole.Admin)
            return Result.Failure<UserResult>(UserErrors.RoleNotAllowed(command.Role.ToString()));

        if (await userRepository.GetByEmailAsync(command.Email, cancellationToken) is not null)
            return Result.Failure<UserResult>(UserErrors.DuplicateEmail(command.Email));

        if (await userRepository.GetByUsernameAsync(command.Username, cancellationToken) is not null)
            return Result.Failure<UserResult>(UserErrors.DuplicateUsername(command.Username));

        var user = User.Register(
            new Username(command.Username),
            new Email(command.Email),
            new Phone(command.Phone),
            new PasswordHash(passwordHasher.HashPassword(command.Password)),
            new PersonName(command.Name.FirstName, command.Name.LastName),
            new Address(
                command.Address.City,
                command.Address.Street,
                command.Address.Number,
                command.Address.ZipCode,
                new Geolocation(command.Address.Geolocation.Lat, command.Address.Geolocation.Long)),
            command.Role,
            command.Status);

        await userRepository.CreateAsync(user, cancellationToken);

        return Result.Success(UserResult.From(user));
    }
}
