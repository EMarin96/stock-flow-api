namespace StockFlow.Infrastructure.Security;

/// <summary>
/// Options Pattern configuration for JWT issuing/validation, bound from the
/// "Jwt" configuration section.
/// </summary>
/// <remarks>
/// <see cref="Secret"/> is a genuine secret, same treatment as
/// <c>CountryStateCityOptions.ApiKey</c> — it must never be committed. Set it
/// locally via <c>dotnet user-secrets set "Jwt:Secret" "&lt;value&gt;"</c>
/// (run from src/Api), or via a CI/deployment secret in other environments.
/// </remarks>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Secret { get; set; } = string.Empty;

    public string Issuer { get; set; } = "StockFlowApi";

    public string Audience { get; set; } = "StockFlowApi";

    /// <summary>
    /// Defaults to 480 minutes (8 hours), per spec.md.
    /// </summary>
    public int ExpiryMinutes { get; set; } = 480;
}
