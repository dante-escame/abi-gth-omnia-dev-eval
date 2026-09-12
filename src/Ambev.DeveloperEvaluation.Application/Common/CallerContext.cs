using Ambev.DeveloperEvaluation.Domain.Enums;

namespace Ambev.DeveloperEvaluation.Application.Common;

public sealed record CallerContext(Guid UserId, UserRole Role, string Name)
{
    public bool IsScoped => Role is not (UserRole.Manager or UserRole.Admin);
}
