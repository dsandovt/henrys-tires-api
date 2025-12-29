using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Documents;

namespace HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Mappings;

public static class InventoryTransactionDocumentMapper
{
    public static InventoryTransaction ToEntity(InventoryTransactionDocument document)
    {
        return new InventoryTransaction
        {
            Id = document.Id,
            TransactionNumber = document.TransactionNumber,
            BranchCode = document.BranchCode,
            Type = document.Type,
            Status = document.Status,
            TransactionDateUtc = document.TransactionDateUtc,
            Notes = document.Notes,
            PaymentMethod = document.PaymentMethod,
            CommittedAtUtc = document.CommittedAtUtc,
            CommittedBy = document.CommittedBy,
            Lines = document.Lines.Select(ToLineEntity).ToList(),
            CreatedAtUtc = document.CreatedAtUtc,
            CreatedBy = document.CreatedBy,
            ModifiedAtUtc = document.ModifiedAtUtc,
            ModifiedBy = document.ModifiedBy
        };
    }

    public static InventoryTransactionDocument ToDocument(InventoryTransaction entity)
    {
        return new InventoryTransactionDocument
        {
            Id = entity.Id,
            TransactionNumber = entity.TransactionNumber,
            BranchCode = entity.BranchCode,
            Type = entity.Type,
            Status = entity.Status,
            TransactionDateUtc = entity.TransactionDateUtc,
            Notes = entity.Notes,
            PaymentMethod = entity.PaymentMethod,
            CommittedAtUtc = entity.CommittedAtUtc,
            CommittedBy = entity.CommittedBy,
            Lines = entity.Lines.Select(ToLineDocument).ToList(),
            CreatedAtUtc = entity.CreatedAtUtc,
            CreatedBy = entity.CreatedBy,
            ModifiedAtUtc = entity.ModifiedAtUtc,
            ModifiedBy = entity.ModifiedBy
        };
    }

    private static InventoryTransactionLine ToLineEntity(InventoryTransactionLineDocument document)
    {
        return new InventoryTransactionLine
        {
            LineId = document.LineId,
            ItemId = document.ItemId,
            ItemCode = document.ItemCode,
            Condition = document.Condition,
            Quantity = document.Quantity,
            UnitPrice = document.UnitPrice,
            Currency = document.Currency,
            IsTaxable = document.IsTaxable,
            AppliesShopFee = document.AppliesShopFee,
            PriceSource = document.PriceSource,
            PriceSetByRole = document.PriceSetByRole,
            PriceSetByUser = document.PriceSetByUser,
            LineTotal = document.LineTotal,
            CostOfGoodsSold = document.CostOfGoodsSold,
            PriceNotes = document.PriceNotes,
            ExecutedAtUtc = document.ExecutedAtUtc
        };
    }

    private static InventoryTransactionLineDocument ToLineDocument(InventoryTransactionLine entity)
    {
        return new InventoryTransactionLineDocument
        {
            LineId = entity.LineId,
            ItemId = entity.ItemId,
            ItemCode = entity.ItemCode,
            Condition = entity.Condition,
            Quantity = entity.Quantity,
            UnitPrice = entity.UnitPrice,
            Currency = entity.Currency,
            IsTaxable = entity.IsTaxable,
            AppliesShopFee = entity.AppliesShopFee,
            PriceSource = entity.PriceSource,
            PriceSetByRole = entity.PriceSetByRole,
            PriceSetByUser = entity.PriceSetByUser,
            LineTotal = entity.LineTotal,
            CostOfGoodsSold = entity.CostOfGoodsSold,
            PriceNotes = entity.PriceNotes,
            ExecutedAtUtc = entity.ExecutedAtUtc
        };
    }
}
