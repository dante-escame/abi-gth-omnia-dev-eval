namespace Ambev.DeveloperEvaluation.WebApi.RateLimiting;

public sealed class LoginRateLimitOptions
{
    public const string SectionName = "RateLimiting:Login";

    public int PermitLimit { get; init; } = 10;

    public int WindowInSeconds { get; init; } = 60;
}
