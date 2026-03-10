using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Documents;

namespace HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Mappings;

public static class ConsumableItemPriceDocumentMapper
{
    public static ConsumableItemPrice ToEntity(ConsumableItemPriceDocument document)
    {
        return new ConsumableItemPrice
        {
            Id = document.Id,
            ItemCode = document.ItemCode,
            Currency = document.Currency,
            LatestPrice = document.LatestPrice,
            LatestPriceDateUtc = document.LatestPriceDateUtc,
            History = document.History.Select(ToHistoryEntity).ToList(),
            CreatedAtUtc = document.CreatedAtUtc,
            CreatedBy = document.CreatedBy,
            ModifiedAtUtc = document.ModifiedAtUtc,
            ModifiedBy = document.ModifiedBy,
        };
    }

    public static ConsumableItemPriceDocument ToDocument(ConsumableItemPrice entity)
    {
        return new ConsumableItemPriceDocument
        {
            Id = entity.Id,
            ItemCode = entity.ItemCode,
            Currency = entity.Currency,
            LatestPrice = entity.LatestPrice,
            LatestPriceDateUtc = entity.LatestPriceDateUtc,
            History = entity.History.Select(ToHistoryDocument).ToList(),
            CreatedAtUtc = entity.CreatedAtUtc,
            CreatedBy = entity.CreatedBy,
            ModifiedAtUtc = entity.ModifiedAtUtc,
            ModifiedBy = entity.ModifiedBy,
        };
    }

    private static PriceHistoryEntry ToHistoryEntity(PriceHistoryEntryDocument document)
    {
        return new PriceHistoryEntry
        {
            Price = document.Price,
            DateUtc = document.DateUtc,
            UpdatedBy = document.UpdatedBy
        };
    }

    private static PriceHistoryEntryDocument ToHistoryDocument(PriceHistoryEntry entity)
    {
        return new PriceHistoryEntryDocument
        {
            Price = entity.Price,
            DateUtc = entity.DateUtc,
            UpdatedBy = entity.UpdatedBy
        };
    }
}
