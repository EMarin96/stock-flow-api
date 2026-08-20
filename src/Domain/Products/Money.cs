namespace StockFlow.Domain.Products;

/// <summary>
/// A monetary amount in a specific <see cref="Products.Currency"/>. Self-validates
/// via guard clauses so a negative amount or an undefined currency is
/// unrepresentable (see plan.md — Decisions).
/// </summary>
public sealed record Money
{
    public decimal Amount { get; }

    public Currency Currency { get; }

    private Money(decimal amount, Currency currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, Currency currency)
    {
        if (amount < 0)
        {
            throw new DomainValidationException("Amount cannot be negative.");
        }

        if (!Enum.IsDefined(typeof(Currency), currency))
        {
            throw new DomainValidationException("Currency is not a valid value.");
        }

        return new Money(amount, currency);
    }
}
