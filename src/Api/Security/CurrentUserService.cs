using System.Security.Claims;
using StockFlow.Application.Common.Security;
using StockFlow.Domain.Users;

namespace StockFlow.Api.Security;

/// <summary>
/// Reads the authenticated caller's identity from the current
/// <see cref="HttpContext"/>'s <see cref="ClaimsPrincipal"/> (populated by the
/// JWT bearer authentication handler). Api-layer concern — same reasoning
/// already documented for <c>AddHttpClient</c>/<c>AddMemoryCache</c>/
/// <c>AddHostedService</c> (see plan.md — Implementation).
/// </summary>
public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid? UserId
    {
        get
        {
            var value = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var userId) ? userId : null;
        }
    }

    public Role? Role
    {
        get
        {
            var value = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role);
            return Enum.TryParse<Role>(value, out var role) ? role : null;
        }
    }
}
