using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Documents;

namespace HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Mappings;

public static class InventorySummaryDocumentMapper
{
    public static InventorySummary ToEntity(InventorySummaryDocument document)
    {
        return new InventorySummary
        {
            Id = document.Id,
            BranchCode = document.BranchCode,
            ItemCode = document.ItemCode,
            Entries = document.Entries.Select(ToEntryEntity).ToList(),
            OnHandTotal = document.OnHandTotal,
            ReservedTotal = document.ReservedTotal,
            Version = document.Version,
            UpdatedAtUtc = document.UpdatedAtUtc
        };
    }

    public static InventorySummaryDocument ToDocument(InventorySummary entity)
    {
        return new InventorySummaryDocument
        {
            Id = entity.Id,
            BranchCode = entity.BranchCode,
            ItemCode = entity.ItemCode,
            Entries = entity.Entries.Select(ToEntryDocument).ToList(),
            OnHandTotal = entity.OnHandTotal,
            ReservedTotal = entity.ReservedTotal,
            Version = entity.Version,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };
    }

    private static InventoryEntry ToEntryEntity(InventoryEntryDocument document)
    {
        return new InventoryEntry
        {
            Condition = document.Condition,
            OnHand = document.OnHand,
            Reserved = document.Reserved,
            LatestEntryDateUtc = document.LatestEntryDateUtc
        };
    }

    private static InventoryEntryDocument ToEntryDocument(InventoryEntry entity)
    {
        return new InventoryEntryDocument
        {
            Condition = entity.Condition,
            OnHand = entity.OnHand,
            Reserved = entity.Reserved,
            LatestEntryDateUtc = entity.LatestEntryDateUtc
        };
    }
}
