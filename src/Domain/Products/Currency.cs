namespace StockFlow.Domain.Products;

/// <summary>
/// The set of currencies a product's <see cref="Money"/> price can be expressed in.
/// A dependency-free enum, so it flows as a typed value through outer layers too
/// (see plan.md — Decisions).
/// </summary>
public enum Currency
{
    USD,
    CRC,
}
