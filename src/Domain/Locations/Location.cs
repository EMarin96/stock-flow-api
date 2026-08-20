namespace StockFlow.Domain.Locations;

/// <summary>
/// A warehouse/storage location. Stock quantity is not stored here — it is
/// calculated per product + location from <c>StockMovement</c> records (out of
/// scope for this feature).
/// </summary>
public class Location : AuditableEntity
{
    public LocationCode Code { get; private set; } = null!;

    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Mandatory — every location has a country/state/city on file. The
    /// address lines are always independently optional (see plan.md —
    /// Decisions).
    /// </summary>
    public Address Address { get; private set; } = null!;

    /// <summary>
    /// Reserved for EF Core materialization.
    /// </summary>
    private Location()
    {
    }

    private Location(LocationCode code, string name, Address address)
    {
        Id = Guid.NewGuid();
        Code = code;
        Name = name;
        Address = address;
        CreatedAt = DateTime.UtcNow;
    }

    public static Location Create(
        string code,
        string name,
        string? addressLine1,
        string? addressLine2,
        string? addressLine3,
        string state,
        string city,
        Country country)
    {
        return new Location(
            LocationCode.Create(code),
            name,
            Address.Create(addressLine1, addressLine2, addressLine3, state, city, country));
    }

    /// <summary>
    /// Updates the editable fields of the location. The code is immutable once
    /// created, same convention as <c>Product.Sku</c>.
    /// </summary>
    public void Update(
        string name,
        string? addressLine1,
        string? addressLine2,
        string? addressLine3,
        string state,
        string city,
        Country country)
    {
        Name = name;
        Address = Address.Create(addressLine1, addressLine2, addressLine3, state, city, country);
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }
}
