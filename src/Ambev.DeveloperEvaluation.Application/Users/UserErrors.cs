using Ambev.DeveloperEvaluation.Application.Common.Results;

namespace Ambev.DeveloperEvaluation.Application.Users;

public static class UserErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        "Users.NotFound",
        "User not found",
        $"The user with ID {id} does not exist in our database");

    public static Error DuplicateEmail(string email) => Error.Conflict(
        "Users.DuplicateEmail",
        "Email already registerd",
        $"Another user is already registered with the email {email}");

    public static Error DuplicateUsername(string username) => Error.Conflict(
        "Users.DuplicateUsername",
        "Username already taken",
        $"Another user is already registered with the username {username}");

    public static Error RoleNotAllowed(string role) => Error.Validation(
        "Users.RoleNotAllowed",
        "Role not allowed on self-registration",
        $"Self registration cannot request the {role} role");

    public static readonly Error RoleChangeForbidden = Error.Forbidden(
        "Users.RoleChangeForbidden",
        "Role change forbidden",
        "Promoting a user to Admin requires an admin caler");

    public static readonly Error RoleOrStatusChangeForbidden = Error.Forbidden(
        "Users.RoleChangeForbidden",
        "Role or status change forbidden",
        "Changing role or status requires a Manager or Admin caller");
}
