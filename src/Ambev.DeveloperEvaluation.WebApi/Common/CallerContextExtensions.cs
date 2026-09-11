using System.Security.Claims;
using Ambev.DeveloperEvaluation.Application.Common;
using Ambev.DeveloperEvaluation.Domain.Enums;

namespace Ambev.DeveloperEvaluation.WebApi.Common;

public static class CallerContextExtensions
{
    public static CallerContext ToCallerContext(this ClaimsPrincipal principal)
    {
        var role = Enum.TryParse<UserRole>(principal.FindFirstValue(ClaimTypes.Role), out var parsed)
            ? parsed
            : UserRole.Customer;

        return new CallerContext(principal.CurrentUserId(), role);
    }

    public static Guid CurrentUserId(this ClaimsPrincipal principal) =>
        Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
            ? userId
            : throw new UnauthorizedAccessException();
}
