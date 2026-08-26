using StockFlow.Application.Common.Security;
using StockFlow.Domain.Users;

namespace StockFlow.Tests.TestDoubles;

/// <summary>
/// In-memory stand-in for <see cref="ICurrentUserService"/> used by handler
/// unit tests, so tests don't depend on <c>HttpContext</c>. Defaults to a
/// fixed, non-null admin identity — tests that specifically exercise
/// anonymous/unauthenticated behavior can pass nulls explicitly.
/// </summary>
public sealed class InMemoryCurrentUserService(Guid? userId = null, Role? role = Role.Admin) : ICurrentUserService
{
    public Guid? UserId { get; } = userId ?? Guid.NewGuid();

    public Role? Role { get; } = role;
}
