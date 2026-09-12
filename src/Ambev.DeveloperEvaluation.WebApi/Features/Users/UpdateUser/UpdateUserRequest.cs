using System.Text.Json.Serialization;
using Ambev.DeveloperEvaluation.Application.Users.UpdateUser;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.WebApi.Features.Users.Common;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Users.UpdateUser;

public class UpdateUserRequest
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string? Password { get; set; }

    [JsonPropertyName("name")]
    public PersonNameRequest Name { get; set; } = new();

    [JsonPropertyName("address")]
    public AddressRequest Address { get; set; } = new();

    [JsonPropertyName("phone")]
    public string Phone { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public UserStatus Status { get; set; }

    [JsonPropertyName("role")]
    public UserRole Role { get; set; }

    public UpdateUserCommand ToCommand(Guid id, UserRole? callerRole) => new()
    {
        Id = id,
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
