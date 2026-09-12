using System.Security.Claims;
using Ambev.DeveloperEvaluation.Application.Common.Lists;
using Ambev.DeveloperEvaluation.Application.Common.Results;
using Ambev.DeveloperEvaluation.Application.Users.DeleteUser;
using Ambev.DeveloperEvaluation.Application.Users.GetUser;
using Ambev.DeveloperEvaluation.Application.Users.ListUsers;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Users.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Users.CreateUser;
using Ambev.DeveloperEvaluation.WebApi.Features.Users.UpdateUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Users;

[ApiController]
[Route("api/[controller]")]
/// <summary>
/// Controller for managing user operations
/// </summary>
/// <param name="mediator">The mediator instance</param>
public class UsersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(ListUsersResponse), StatusCodes.Status200OK)]
    /// <summary>
    /// Retrieves a paged list of users
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The page of users, with the total item and page counts</returns>
    public async Task<IActionResult> ListUsers(CancellationToken cancellationToken)
    {
        var parameters = Request.Query.ToDictionary(entry => entry.Key, entry => (string?)entry.Value.ToString());
        var page = await mediator.Send(new ListUsersQuery(ListQueryParser.Parse(parameters)), cancellationToken);

        return Ok(ListUsersResponse.From(page));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    /// <summary>
    /// Retrieves a user by their ID
    /// </summary>
    /// <param name="id">The unique identifier of the user</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The user details if found</returns>
    public async Task<IActionResult> GetUser([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetUserCommand(id), cancellationToken);

        return result.IsSuccess
            ? Ok(UserResponse.From(result.Value))
            : Problem(result.Error!);
    }

    [HttpPost]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    /// <summary>
    /// Creates a new user
    /// </summary>
    /// <param name="request">The user creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created user details</returns>
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request.ToCommand(CallerRole()), cancellationToken);

        if (result.IsFailure)
            return Problem(result.Error!);

        var response = UserResponse.From(result.Value);

        return CreatedAtAction(nameof(GetUser), new { id = response.Id }, response);
    }

    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    /// <summary>
    /// Replaces the mutable fields of an existing user
    /// </summary>
    /// <param name="id">The unique identifier of the user to update</param>
    /// <param name="request">The user update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated user details</returns>
    public async Task<IActionResult> UpdateUser(
        [FromRoute] Guid id,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request.ToCommand(id, CallerRole()), cancellationToken);

        return result.IsSuccess
            ? Ok(UserResponse.From(result.Value))
            : Problem(result.Error!);
    }

    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    /// <summary>
    /// Deletes a user by their ID
    /// </summary>
    /// <param name="id">The unique identifier of the user to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content if the user was deleted</returns>
    public async Task<IActionResult> DeleteUser([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteUserCommand(id), cancellationToken);

        return result.IsSuccess ? NoContent() : Problem(result.Error!);
    }

    /// <summary>
    /// Reads the role carried by the caller's token, when the request is authenticated
    /// </summary>
    /// <returns>The caller's role, or null when absent or unrecognised</returns>
    private UserRole? CallerRole() =>
        Enum.TryParse<UserRole>(User.FindFirstValue(ClaimTypes.Role), out var role) ? role : null;

    /// <summary>
    /// Writes a failed result as the standard error body with its mapped status code
    /// </summary>
    /// <param name="error">The error carried by the failed result</param>
    /// <returns>The error response</returns>
    private IActionResult Problem(Error error) =>
        StatusCode(ApiResults.StatusFor(error.Category), ApiResults.ToResponse(error));
}
