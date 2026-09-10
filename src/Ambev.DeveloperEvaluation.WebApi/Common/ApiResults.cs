using Ambev.DeveloperEvaluation.Application.Common.Results;
using Microsoft.AspNetCore.Http;

namespace Ambev.DeveloperEvaluation.WebApi.Common;

public static class ApiResults
{
    public static int StatusFor(ErrorCategory category) => category switch
    {
        ErrorCategory.Validation => StatusCodes.Status400BadRequest,
        ErrorCategory.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorCategory.Forbidden => StatusCodes.Status403Forbidden,
        ErrorCategory.NotFound => StatusCodes.Status404NotFound,
        ErrorCategory.Conflict => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status500InternalServerError
    };

    public static ErrorResponse ToResponse(Error error) => new(error.Type, error.Summary, error.Detail);
}
