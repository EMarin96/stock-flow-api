using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StockFlow.Application.Common.Security;
using StockFlow.Domain.Users;
using StockFlow.Infrastructure.Security;

namespace StockFlow.Api.Security;

/// <summary>
/// Registers authentication (JWT bearer) and authorization (role-based
/// policies) services. Kept separate from each other since they're two
/// distinct ASP.NET Core concepts, even though both arrived with feature 004.
/// </summary>
public static class AuthServiceCollectionExtensions
{
    /// <summary>
    /// JwtOptions is bound via the Options Pattern and consumed lazily — both by
    /// JwtTokenGenerator (through IOptions&lt;JwtOptions&gt;) and by the JwtBearer
    /// handler's TokenValidationParameters below, via a deferred PostConfigure
    /// callback rather than a value captured eagerly at startup. This matters for
    /// integration tests: ApiFactory overrides Jwt:Secret via
    /// ConfigureAppConfiguration, and that override is only visible to code that
    /// resolves IConfiguration/IOptions&lt;T&gt; through DI *after* the host finishes
    /// building — never to a plain local variable read synchronously from
    /// builder.Configuration before Build() (same reasoning already documented
    /// for CountryStateCityOptions, consumed lazily inside the AddHttpClient
    /// factory callback).
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((bearerOptions, jwtOptionsAccessor) =>
            {
                var jwtOptions = jwtOptionsAccessor.Value;
                bearerOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                };
            });

        return services;
    }

    /// <summary>
    /// Fail-closed by default: every endpoint requires an authenticated user
    /// unless explicitly marked AllowAnonymous() (only POST /api/auth/login).
    /// WriteAccess/AdminOnly are applied per-endpoint on top of this default (see
    /// plan.md — Decisions).
    /// </summary>
    public static IServiceCollection AddRoleAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            options.AddPolicy(nameof(AuthorizationPolicy.WriteAccess), policy => policy.RequireRole(nameof(Role.Admin), nameof(Role.Operator)));
            options.AddPolicy(nameof(AuthorizationPolicy.AdminOnly), policy => policy.RequireRole(nameof(Role.Admin)));
        });

        return services;
    }
}
