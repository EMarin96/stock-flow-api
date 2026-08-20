namespace StockFlow.Domain.Locations;

/// <summary>
/// A location/warehouse's unique code. Self-validates via guard clauses so an
/// invalid code is unrepresentable (see plan.md — Decisions).
/// </summary>
public sealed record LocationCode
{
    public string Value { get; }

    private LocationCode(string value)
    {
        Value = value;
    }

    public static LocationCode Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainValidationException("Location code cannot be empty.");
        }

        if (value.Length > 32)
        {
            throw new DomainValidationException("Location code cannot be longer than 32 characters.");
        }

        return new LocationCode(value);
    }
}
