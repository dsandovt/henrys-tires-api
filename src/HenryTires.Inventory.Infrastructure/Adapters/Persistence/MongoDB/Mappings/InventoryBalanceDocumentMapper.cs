using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Documents;

namespace HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Mappings;

public static class InventoryBalanceDocumentMapper
{
    public static InventoryBalance ToEntity(InventoryBalanceDocument document)
    {
        return new InventoryBalance
        {
            Id = document.Id,
            BranchId = document.BranchId,
            ItemId = document.ItemId,
            ItemCode = document.ItemCode,
            Condition = document.Condition,
            QuantityOnHand = document.QuantityOnHand,
            LastUpdatedUtc = document.LastUpdatedUtc
        };
    }

    public static InventoryBalanceDocument ToDocument(InventoryBalance entity)
    {
        return new InventoryBalanceDocument
        {
            Id = entity.Id,
            BranchId = entity.BranchId,
            ItemId = entity.ItemId,
            ItemCode = entity.ItemCode,
            Condition = entity.Condition,
            QuantityOnHand = entity.QuantityOnHand,
            LastUpdatedUtc = entity.LastUpdatedUtc
        };
    }
}
