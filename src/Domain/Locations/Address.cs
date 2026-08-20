namespace StockFlow.Domain.Locations;

/// <summary>
/// A physical address. <see cref="AddressLine1"/>/<see cref="AddressLine2"/>/
/// <see cref="AddressLine3"/> are always independently optional free text and
/// never participate in any cross-field rule. <see cref="Country"/>,
/// <see cref="State"/>, and <see cref="City"/> are unconditionally required —
/// every address has a real country/state/city on file. Self-validates via
/// guard clauses so a missing state/city is unrepresentable (see plan.md —
/// Decisions). Domain has no I/O, so it can only enforce presence — confirming
/// that <see cref="State"/>/<see cref="City"/> are *real*, recognized values
/// for the given <see cref="Country"/> is validated separately, in the
/// Application layer, via ICountryReferenceDataService.
/// </summary>
public sealed record Address
{
    public string? AddressLine1 { get; }

    public string? AddressLine2 { get; }

    public string? AddressLine3 { get; }

    public string State { get; }

    public string City { get; }

    public Country Country { get; }

    private Address(string? addressLine1, string? addressLine2, string? addressLine3, string state, string city, Country country)
    {
        AddressLine1 = addressLine1;
        AddressLine2 = addressLine2;
        AddressLine3 = addressLine3;
        State = state;
        City = city;
        Country = country;
    }

    public static Address Create(
        string? addressLine1,
        string? addressLine2,
        string? addressLine3,
        string state,
        string city,
        Country country)
    {
        if (addressLine1 is { Length: > 200 })
        {
            throw new DomainValidationException("Address line 1 cannot be longer than 200 characters.");
        }

        if (addressLine2 is { Length: > 200 })
        {
            throw new DomainValidationException("Address line 2 cannot be longer than 200 characters.");
        }

        if (addressLine3 is { Length: > 200 })
        {
            throw new DomainValidationException("Address line 3 cannot be longer than 200 characters.");
        }

        if (string.IsNullOrWhiteSpace(state))
        {
            throw new DomainValidationException("State cannot be empty.");
        }

        if (state.Length > 10)
        {
            throw new DomainValidationException("State cannot be longer than 10 characters.");
        }

        if (string.IsNullOrWhiteSpace(city))
        {
            throw new DomainValidationException("City cannot be empty.");
        }

        if (city.Length > 100)
        {
            throw new DomainValidationException("City cannot be longer than 100 characters.");
        }

        return new Address(addressLine1, addressLine2, addressLine3, state, city, country);
    }
}
