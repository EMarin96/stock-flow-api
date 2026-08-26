namespace StockFlow.Api.Security;

/// <summary>
/// Names of the ASP.NET Core authorization policies registered in
/// <c>Program.cs</c>. Referenced everywhere via <c>nameof(...)</c> instead of
/// a hardcoded string, so a rename breaks the build at every call site
/// instead of silently drifting (see plan.md — Decisions).
/// </summary>
public enum AuthorizationPolicy
{
    WriteAccess,
    AdminOnly,
}
