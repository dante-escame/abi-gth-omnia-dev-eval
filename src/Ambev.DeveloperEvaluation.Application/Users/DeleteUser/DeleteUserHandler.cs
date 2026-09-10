using Ambev.DeveloperEvaluation.Application.Common.Results;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Users.DeleteUser;

/// <summary>
/// Handler for processing DeleteUserCommand requests
/// </summary>
/// <param name="userRepository">The user repository</param>
public sealed class DeleteUserHandler(IUserRepository userRepository)
    : IRequestHandler<DeleteUserCommand, Result>
{
    /// <summary>
    /// Handles the DeleteUserCommand request
    /// </summary>
    /// <param name="request">The DeleteUser command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the delete operation</returns>
    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var deleted = await userRepository.DeleteAsync(request.Id, cancellationToken);

        return deleted
            ? Result.Success()
            : Result.Failure(UserErrors.NotFound(request.Id));
    }
}
