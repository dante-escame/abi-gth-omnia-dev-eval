using Ambev.DeveloperEvaluation.Application.Common.Results;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Users.DeleteUser;

/// <summary>
/// Command for deleting a user
/// </summary>
/// <param name="Id">The unique identifier of the user to delete</param>
public sealed record DeleteUserCommand(Guid Id) : IRequest<Result>;
