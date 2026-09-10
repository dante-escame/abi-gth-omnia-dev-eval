using Ambev.DeveloperEvaluation.Application.Common.Results;
using Ambev.DeveloperEvaluation.Application.Users.Common;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Users.GetUser;

/// <summary>
/// Command for retrieving a user by their ID
/// </summary>
/// <param name="Id">The unique identifier of the user to retrieve</param>
public sealed record GetUserCommand(Guid Id) : IRequest<Result<UserResult>>;
