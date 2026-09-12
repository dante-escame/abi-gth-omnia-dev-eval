namespace Ambev.DeveloperEvaluation.Integration.Users;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class LoginRateLimitApiFactory : UsersApiFactory
{
    public const int PermitLimit = 10;

    protected override int LoginPermitLimit => PermitLimit;
}
