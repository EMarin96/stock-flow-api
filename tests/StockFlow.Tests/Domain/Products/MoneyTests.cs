using StockFlow.Domain.Common;
using StockFlow.Domain.Products;

namespace StockFlow.Tests.Domain.Products;

public class MoneyTests
{
    [Fact]
    public void Create_WithValidAmountAndCurrency_CreatesMoney()
    {
        var money = Money.Create(10m, Currency.USD);

        Assert.Equal(10m, money.Amount);
        Assert.Equal(Currency.USD, money.Currency);
    }

    [Fact]
    public void Create_WithZeroAmount_CreatesMoney()
    {
        var money = Money.Create(0m, Currency.CRC);

        Assert.Equal(0m, money.Amount);
    }

    [Fact]
    public void Create_WhenAmountIsNegative_Throws()
    {
        Assert.Throws<DomainValidationException>(() => Money.Create(-1m, Currency.USD));
    }

    [Fact]
    public void Create_WhenCurrencyIsUndefined_Throws()
    {
        Assert.Throws<DomainValidationException>(() => Money.Create(10m, (Currency)999));
    }
}
