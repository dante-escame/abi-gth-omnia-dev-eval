using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Ambev.DeveloperEvaluation.Common.Security
{
    public static class AuthenticationExtension
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

            var secretKey = configuration["Jwt:SecretKey"]?.ToString();
            ArgumentException.ThrowIfNullOrWhiteSpace(secretKey);

            var key = Encoding.ASCII.GetBytes(secretKey);

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };

                x.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse();

                        return WriteError(
                            context.Response,
                            StatusCodes.Status401Unauthorized,
                            "AuthenticationError",
                            "Invalid authentication",
                            "The provided credentials are invalid or the session has expired");
                    },
                    OnForbidden = context => WriteError(
                        context.Response,
                        StatusCodes.Status403Forbidden,
                        "AuthorizationError",
                        "Access denied",
                        "Your role does not allow this operation")
                };
            });

            return services;
        }

        private static Task WriteError(HttpResponse response, int status, string type, string error, string detail)
        {
            response.StatusCode = status;
            response.ContentType = "application/json";

            return response.WriteAsJsonAsync(new { type, error, detail });
        }
    }
}
