namespace StockFlow.Domain.Products;

/// <summary>
/// A catalog product. Current stock is not stored on the product itself — it is
/// calculated from its <c>StockMovement</c> records (out of scope for this feature).
/// </summary>
public class Product : AuditableEntity
{
    public Sku Sku { get; private set; } = null!;

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public string UnitOfMeasure { get; private set; } = string.Empty;

    public Money Price { get; private set; } = null!;

    public int MinimumStockThreshold { get; private set; }

    /// <summary>
    /// Reserved for EF Core materialization.
    /// </summary>
    private Product()
    {
    }

    private Product(
        Sku sku,
        string name,
        string? description,
        string unitOfMeasure,
        Money price,
        int minimumStockThreshold,
        Guid? createdBy)
    {
        Id = Guid.NewGuid();
        Sku = sku;
        Name = name;
        Description = description;
        UnitOfMeasure = unitOfMeasure;
        Price = price;
        MinimumStockThreshold = minimumStockThreshold;
        CreatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;
    }

    public static Product Create(
        string sku,
        string name,
        string? description,
        string unitOfMeasure,
        decimal price,
        Currency currency,
        int minimumStockThreshold,
        Guid? createdBy = null)
    {
        return new Product(Sku.Create(sku), name, description, unitOfMeasure, Money.Create(price, currency), minimumStockThreshold, createdBy);
    }

    /// <summary>
    /// Updates the editable fields of the product. The SKU is immutable once created.
    /// </summary>
    public void Update(
        string name,
        string? description,
        string unitOfMeasure,
        decimal price,
        Currency currency,
        int minimumStockThreshold,
        Guid? updatedBy = null)
    {
        Name = name;
        Description = description;
        UnitOfMeasure = unitOfMeasure;
        Price = Money.Create(price, currency);
        MinimumStockThreshold = minimumStockThreshold;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }
}
