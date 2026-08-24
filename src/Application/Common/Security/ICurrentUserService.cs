using StockFlow.Domain.Users;

namespace StockFlow.Application.Common.Security;

/// <summary>
/// Exposes the identity of the caller of the current request, sourced from
/// the authenticated JWT. Implemented in Api (via <c>IHttpContextAccessor</c>,
/// see plan.md — Implementation) so Application stays free of ASP.NET Core
/// (<c>HttpContext</c>) specifics, same boundary already drawn for
/// <c>ICountryReferenceDataService</c>. Both members are <c>null</c> when
/// there is no authenticated user (e.g. the startup admin-bootstrap block).
/// </summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }

    Role? Role { get; }
}
