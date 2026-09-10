using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.Domain.Common;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;

namespace Ambev.DeveloperEvaluation.WebApi.Middleware;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, body) = Translate(exception);

        if (status == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled exception for {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(body, cancellationToken);

        return true;
    }

    private static (int Status, ErrorResponse Body) Translate(Exception exception) => exception switch
    {
        ValidationException validation => (
            StatusCodes.Status400BadRequest,
            new ErrorResponse(
                "ValidationError",
                "Invalid input data",
                string.Join(" ", validation.Errors.Select(error => error.ErrorMessage)))),
        DomainException domain => (
            StatusCodes.Status400BadRequest,
            new ErrorResponse("ValidationError", "Invalid input data", domain.Message)),
        UnauthorizedAccessException => (
            StatusCodes.Status401Unauthorized,
            new ErrorResponse(
                "AuthenticationError",
                "Invalid authentication",
                "The provided credentials are invalid or the session has expired")),
        _ => (
            StatusCodes.Status500InternalServerError,
            new ErrorResponse(
                "InternalServerError",
                "An unexpected error occurred",
                "The request could not be processed. Please try again later"))
    };
}
