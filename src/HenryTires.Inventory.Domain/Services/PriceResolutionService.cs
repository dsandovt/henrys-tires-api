using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Domain.Enums;

namespace HenryTires.Inventory.Domain.Services;

public class PriceResolutionService
{
    public Money? GetCurrentPrice(ConsumableItemPrice? price)
    {
        if (price == null)
            return null;

        return new Money(price.LatestPrice, price.Currency);
    }
}

public class Money
{
    public decimal Amount { get; }
    public Currency Currency { get; }

    public Money(decimal amount, Currency currency)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));

        Amount = amount;
        Currency = currency;
    }

    public static Money Usd(decimal amount) => new Money(amount, Currency.USD);
}
