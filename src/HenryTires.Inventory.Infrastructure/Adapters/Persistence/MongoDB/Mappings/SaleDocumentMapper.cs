using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Documents;

namespace HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Mappings;

public static class SaleDocumentMapper
{
    public static Sale ToEntity(SaleDocument document)
    {
        return new Sale
        {
            Id = document.Id,
            SaleNumber = document.SaleNumber,
            BranchId = document.BranchId,
            SaleDateUtc = document.SaleDateUtc,
            Lines = document.Lines.Select(ToLineEntity).ToList(),
            CustomerName = document.CustomerName,
            CustomerPhone = document.CustomerPhone,
            Notes = document.Notes,
            Status = document.Status,
            PostedAtUtc = document.PostedAtUtc,
            PostedBy = document.PostedBy,
            CreatedAtUtc = document.CreatedAtUtc,
            CreatedBy = document.CreatedBy,
            ModifiedAtUtc = document.ModifiedAtUtc,
            ModifiedBy = document.ModifiedBy
        };
    }

    public static SaleDocument ToDocument(Sale entity)
    {
        return new SaleDocument
        {
            Id = entity.Id,
            SaleNumber = entity.SaleNumber,
            BranchId = entity.BranchId,
            SaleDateUtc = entity.SaleDateUtc,
            Lines = entity.Lines.Select(ToLineDocument).ToList(),
            CustomerName = entity.CustomerName,
            CustomerPhone = entity.CustomerPhone,
            Notes = entity.Notes,
            Status = entity.Status,
            PostedAtUtc = entity.PostedAtUtc,
            PostedBy = entity.PostedBy,
            CreatedAtUtc = entity.CreatedAtUtc,
            CreatedBy = entity.CreatedBy,
            ModifiedAtUtc = entity.ModifiedAtUtc,
            ModifiedBy = entity.ModifiedBy
        };
    }

    private static SaleLine ToLineEntity(SaleLineDocument document)
    {
        return new SaleLine
        {
            LineId = document.LineId,
            ItemId = document.ItemId,
            ItemCode = document.ItemCode,
            Description = document.Description,
            Classification = document.Classification,
            Condition = document.Condition,
            Quantity = document.Quantity,
            UnitPrice = document.UnitPrice,
            Currency = document.Currency,
            InventoryTransactionId = document.InventoryTransactionId
        };
    }

    private static SaleLineDocument ToLineDocument(SaleLine entity)
    {
        return new SaleLineDocument
        {
            LineId = entity.LineId,
            ItemId = entity.ItemId,
            ItemCode = entity.ItemCode,
            Description = entity.Description,
            Classification = entity.Classification,
            Condition = entity.Condition,
            Quantity = entity.Quantity,
            UnitPrice = entity.UnitPrice,
            Currency = entity.Currency,
            InventoryTransactionId = entity.InventoryTransactionId
        };
    }
}
