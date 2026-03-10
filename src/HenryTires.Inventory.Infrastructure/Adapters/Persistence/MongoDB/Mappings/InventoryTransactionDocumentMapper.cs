using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Domain.ValueObjects;
using HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Documents;

namespace HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Mappings;

public static class InventoryTransactionDocumentMapper
{
    public static InventoryTransaction ToEntity(InventoryTransactionDocument document)
    {
        return new InventoryTransaction
        {
            Id = document.Id,
            BranchReference = document.BranchReference,
            BranchCode = document.BranchCode,
            Initiator = new EntityKey
            {
                Reference = document.Initiator.Reference,
                ReferenceNumber = document.Initiator.ReferenceNumber,
                EntityDefinitionCode = document.Initiator.EntityDefinitionCode,
            },
            Status = document.Status,
            TransactionDateUtc = document.TransactionDateUtc,
            Notes = document.Notes,
            Lines = document.Lines.Select(ToLineEntity).ToList(),
            StatusHistory = document.StatusHistory.Select(StatusHistoryMapper.ToEntity).ToList(),
            CreatedAtUtc = document.CreatedAtUtc,
            CreatedBy = document.CreatedBy,
            ModifiedAtUtc = document.ModifiedAtUtc,
            ModifiedBy = document.ModifiedBy,
        };
    }

    public static InventoryTransactionDocument ToDocument(InventoryTransaction entity)
    {
        return new InventoryTransactionDocument
        {
            Id = entity.Id,
            BranchReference = entity.BranchReference,
            BranchCode = entity.BranchCode,
            Initiator = new EntityKeyDocument
            {
                Reference = entity.Initiator.Reference,
                ReferenceNumber = entity.Initiator.ReferenceNumber,
                EntityDefinitionCode = entity.Initiator.EntityDefinitionCode,
            },
            Status = entity.Status,
            TransactionDateUtc = entity.TransactionDateUtc,
            Notes = entity.Notes,
            Lines = entity.Lines.Select(ToLineDocument).ToList(),
            StatusHistory = entity.StatusHistory.Select(StatusHistoryMapper.ToDocument).ToList(),
            CreatedAtUtc = entity.CreatedAtUtc,
            CreatedBy = entity.CreatedBy,
            ModifiedAtUtc = entity.ModifiedAtUtc,
            ModifiedBy = entity.ModifiedBy,
        };
    }

    private static InventoryTransactionLine ToLineEntity(InventoryTransactionLineDocument document)
    {
        return new InventoryTransactionLine
        {
            LineId = document.LineId,
            ItemCode = document.ItemCode,
            Condition = document.Condition,
            Quantity = document.Quantity,
        };
    }

    private static InventoryTransactionLineDocument ToLineDocument(InventoryTransactionLine entity)
    {
        return new InventoryTransactionLineDocument
        {
            LineId = entity.LineId,
            ItemCode = entity.ItemCode,
            Condition = entity.Condition,
            Quantity = entity.Quantity,
        };
    }
}
