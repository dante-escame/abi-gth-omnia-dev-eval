using Ambev.DeveloperEvaluation.Application.Common.Results;
using Ambev.DeveloperEvaluation.Application.Users.Common;
using Ambev.DeveloperEvaluation.Domain.Enums;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Users.CreateUser;

/// <summary>
/// Command for creating a new user.
/// </summary>
/// <remarks>
/// This command is used to capture the required data for creating a user,
/// including username, password, phone number, email, name, address, status, and role.
/// It implements <see cref="IRequest{TResponse}"/> to initiate the request
/// that returns a <see cref="Result{TValue}"/> carrying a <see cref="UserResult"/>.
///
/// The data provided in this command is validated using the
/// <see cref="CreateUserCommandValidator"/> which extends
/// <see cref="AbstractValidator{T}"/> to ensure that the fields are correctly
/// populated and follow the required rules.
/// </remarks>
public class CreateUserCommand : IRequest<Result<UserResult>>
{
    /// <summary>
    /// Gets or sets the username of the user to be created.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password for the user.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the phone number for the user.
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email address for the user.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the first and last name of the user.
    /// </summary>
    public PersonNameInput Name { get; set; } = PersonNameInput.Empty;

    /// <summary>
    /// Gets or sets the postal address of the user, including its geolocation.
    /// </summary>
    public AddressInput Address { get; set; } = AddressInput.Empty;

    /// <summary>
    /// Gets or sets the status of the user.
    /// </summary>
    public UserStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the role of the user.
    /// </summary>
    public UserRole Role { get; set; }

    /// <summary>
    /// Gets or sets the role carried by the caller's token, when the request is authenticated.
    /// Only an Admin caller may create a user with an elevated role.
    /// </summary>
    public UserRole? CallerRole { get; set; }
}
