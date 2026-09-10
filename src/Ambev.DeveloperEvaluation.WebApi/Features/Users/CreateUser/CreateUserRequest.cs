using System.Text.Json.Serialization;
using Ambev.DeveloperEvaluation.Application.Users.CreateUser;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.WebApi.Features.Users.Common;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Users.CreateUser;

/// <summary>
/// Represents a request to create a new user in the system.
/// </summary>
public class CreateUserRequest
{
    [JsonPropertyName("email")]
    /// <summary>
    /// Gets or sets the email address. Must be a valid email format.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("username")]
    /// <summary>
    /// Gets or sets the username. Must be unique and contain only valid characters.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    /// <summary>
    /// Gets or sets the password. Must meet security requirements.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    /// <summary>
    /// Gets or sets the first and last name of the user.
    /// </summary>
    public PersonNameRequest Name { get; set; } = new();

    [JsonPropertyName("address")]
    /// <summary>
    /// Gets or sets the postal address, including its geolocation.
    /// </summary>
    public AddressRequest Address { get; set; } = new();

    [JsonPropertyName("phone")]
    /// <summary>
    /// Gets or sets the phone number in international format.
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    /// <summary>
    /// Gets or sets the initial status of the user account.
    /// </summary>
    public UserStatus Status { get; set; }

    [JsonPropertyName("role")]
    /// <summary>
    /// Gets or sets the role assigned to the user.
    /// </summary>
    public UserRole Role { get; set; }

    /// <summary>
    /// Converts the request into the command handled by the application layer.
    /// </summary>
    /// <param name="callerRole">The role carried by the caller's token, when authenticated</param>
    /// <returns>The create user command</returns>
    public CreateUserCommand ToCommand(UserRole? callerRole) => new()
    {
        Email = Email,
        Username = Username,
        Password = Password,
        Name = Name.ToInput(),
        Address = Address.ToInput(),
        Phone = Phone,
        Status = Status,
        Role = Role,
        CallerRole = callerRole
    };
}
