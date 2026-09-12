using Ambev.DeveloperEvaluation.Application.Common.Lists;
using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Application.Users.Common;

public static class UserListFields
{
    public static readonly ListFieldMap<User> Map = new ListFieldMap<User>()
        .Map("id", user => user.Id)
        .Map("email", user => user.Email.Value)
        .Map("username", user => user.Username.Value)
        .Map("password", user => user.Password.Value)
        .Map("phone", user => user.Phone.Value)
        .Map("status", user => user.Status)
        .Map("role", user => user.Role);
}
