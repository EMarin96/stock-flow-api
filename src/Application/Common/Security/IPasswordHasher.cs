namespace StockFlow.Application.Common.Security;

/// <summary>
/// Hashes and verifies user passwords. Implemented in Infrastructure (PBKDF2,
/// see plan.md — Decisions) so Application stays free of cryptography-library
/// specifics, same boundary already drawn for <c>ICountryReferenceDataService</c>.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string passwordHash);
}
