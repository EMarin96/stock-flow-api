namespace StockFlow.Domain.Locations;

/// <summary>
/// The set of countries StockFlow currently supports for a <see cref="Location"/>'s
/// <see cref="Address"/>. A closed, dependency-free enum — no external call is
/// needed for <see cref="Country"/> itself, unlike <see cref="Address.State"/>/
/// <see cref="Address.City"/>, which are validated against real reference data
/// (see plan.md — Decisions).
/// </summary>
public enum Country
{
    US,
    CR,
}
