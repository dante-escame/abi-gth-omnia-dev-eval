using System.Globalization;
using System.Threading.RateLimiting;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Microsoft.Extensions.Options;

namespace Ambev.DeveloperEvaluation.WebApi.RateLimiting;

public static class RateLimitingExtension
{
    private const string UnknownClient = "unknown";

    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IServiceCollection AddLoginRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<LoginRateLimitOptions>(configuration.GetSection(LoginRateLimitOptions.SectionName));

        services.AddRateLimiter(limiter =>
        {
            limiter.AddPolicy(RateLimitPolicies.Login, context =>
            {
                var options = context.RequestServices.GetRequiredService<IOptions<LoginRateLimitOptions>>().Value;

                return RateLimitPartition.GetFixedWindowLimiter(
                    ClientKey(context),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = options.PermitLimit,
                        Window = TimeSpan.FromSeconds(options.WindowInSeconds),
                        QueueLimit = 0
                    });
            });

            limiter.OnRejected = (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    context.HttpContext.Response.Headers.RetryAfter =
                        ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString(CultureInfo.InvariantCulture);

                return new ValueTask(context.HttpContext.Response.WriteAsJsonAsync(
                    new ErrorResponse(
                        "TooManyRequests",
                        "Too many requests",
                        "Too many attempts from this client. Please wait before trying again"),
                    cancellationToken));
            };
        });

        return services;
    }

    private static string ClientKey(HttpContext context) =>
        context.Connection.RemoteIpAddress?.ToString() ?? UnknownClient;
}
