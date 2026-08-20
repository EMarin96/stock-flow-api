namespace StockFlow.Domain.Products;

/// <summary>
/// A product's stock-keeping unit code. Self-validates via guard clauses so an
/// invalid SKU is unrepresentable (see plan.md — Decisions).
/// </summary>
public sealed record Sku
{
    public string Value { get; }

    private Sku(string value)
    {
        Value = value;
    }

    public static Sku Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainValidationException("SKU cannot be empty.");
        }

        if (value.Length > 64)
        {
            throw new DomainValidationException("SKU cannot be longer than 64 characters.");
        }

        return new Sku(value);
    }
}
