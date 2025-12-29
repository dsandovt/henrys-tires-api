using HenryTires.Inventory.Domain.Entities;

namespace HenryTires.Inventory.Application.DTOs;

public class ConsumableItemPriceDto
{
    public required string Id { get; set; }
    public required string ItemCode { get; set; }
    public required string Currency { get; set; }
    public required decimal LatestPrice { get; set; }
    public required DateTime LatestPriceDateUtc { get; set; }
    public required string UpdatedBy { get; set; }

    public static ConsumableItemPriceDto FromEntity(ConsumableItemPrice price)
    {
        return new ConsumableItemPriceDto
        {
            Id = price.Id,
            ItemCode = price.ItemCode,
            Currency = price.Currency,
            LatestPrice = price.LatestPrice,
            LatestPriceDateUtc = price.LatestPriceDateUtc,
            UpdatedBy = price.UpdatedBy,
        };
    }
}

public class UpdateItemPriceRequest
{
    public required decimal NewPrice { get; set; }
    public required string Currency { get; set; }
}

public class PriceHistoryDto
{
    public required decimal Price { get; set; }
    public required DateTime DateUtc { get; set; }
    public required string UpdatedBy { get; set; }

    public static PriceHistoryDto FromEntity(PriceHistoryEntry entry)
    {
        return new PriceHistoryDto
        {
            Price = entry.Price,
            DateUtc = entry.DateUtc,
            UpdatedBy = entry.UpdatedBy,
        };
    }
}

public class ConsumableItemPriceWithHistoryDto
{
    public required string Id { get; set; }
    public required string ItemCode { get; set; }
    public required string Currency { get; set; }
    public required decimal LatestPrice { get; set; }
    public required DateTime LatestPriceDateUtc { get; set; }
    public required string UpdatedBy { get; set; }
    public required List<PriceHistoryDto> History { get; set; }

    public static ConsumableItemPriceWithHistoryDto FromEntity(ConsumableItemPrice price)
    {
        return new ConsumableItemPriceWithHistoryDto
        {
            Id = price.Id,
            ItemCode = price.ItemCode,
            Currency = price.Currency,
            LatestPrice = price.LatestPrice,
            LatestPriceDateUtc = price.LatestPriceDateUtc,
            UpdatedBy = price.UpdatedBy,
            History = price.History.Select(PriceHistoryDto.FromEntity).ToList(),
        };
    }
}
