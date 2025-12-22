using HenryTires.Inventory.Domain.Entities;

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
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be empty", nameof(currency));

        Amount = amount;
        Currency = currency;
    }

    public static Money Usd(decimal amount) => new Money(amount, "USD");
}
