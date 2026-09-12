using Ambev.DeveloperEvaluation.Application.Common.Results;
using Ambev.DeveloperEvaluation.Application.Users.Common;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Users.GetUser;

/// <summary>
/// Handler for processing GetUserCommand requests
/// </summary>
/// <param name="userRepository">The user repository</param>
public sealed class GetUserHandler(IUserRepository userRepository)
    : IRequestHandler<GetUserCommand, Result<UserResult>>
{
    /// <summary>
    /// Handles the GetUserCommand request
    /// </summary>
    /// <param name="request">The GetUser command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The user details if found</returns>
    public async Task<Result<UserResult>> Handle(GetUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.Id, cancellationToken);

        return user is null
            ? Result.Failure<UserResult>(UserErrors.NotFound(request.Id))
            : Result.Success(UserResult.From(user));
    }
}
