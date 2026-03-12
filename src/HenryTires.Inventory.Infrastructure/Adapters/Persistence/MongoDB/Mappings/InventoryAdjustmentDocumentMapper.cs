using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Documents;

namespace HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Mappings;

public static class InventoryAdjustmentDocumentMapper
{
    public static InventoryAdjustment ToEntity(InventoryAdjustmentDocument document)
    {
        return new InventoryAdjustment
        {
            Id = document.Id,
            Number = document.Number,
            AdjustmentType = document.AdjustmentType,
            Status = document.Status,
            StatusHistory = document.StatusHistory.Select(StatusHistoryMapper.ToEntity).ToList(),
            AdjustmentDateUtc = document.AdjustmentDateUtc,
            Notes = document.Notes,
            OriginBranchReference = document.OriginBranchReference,
            OriginBranchCode = document.OriginBranchCode,
            DestinationBranchReference = document.DestinationBranchReference,
            DestinationBranchCode = document.DestinationBranchCode,
            BranchReference = document.BranchReference,
            BranchCode = document.BranchCode,
            Direction = document.Direction,
            Lines = document.Lines.Select(ToLineEntity).ToList(),
            CreatedAtUtc = document.CreatedAtUtc,
            CreatedBy = document.CreatedBy,
            ModifiedAtUtc = document.ModifiedAtUtc,
            ModifiedBy = document.ModifiedBy,
        };
    }

    public static InventoryAdjustmentDocument ToDocument(InventoryAdjustment entity)
    {
        return new InventoryAdjustmentDocument
        {
            Id = entity.Id,
            Number = entity.Number,
            AdjustmentType = entity.AdjustmentType,
            Status = entity.Status,
            StatusHistory = entity.StatusHistory.Select(StatusHistoryMapper.ToDocument).ToList(),
            AdjustmentDateUtc = entity.AdjustmentDateUtc,
            Notes = entity.Notes,
            OriginBranchReference = entity.OriginBranchReference,
            OriginBranchCode = entity.OriginBranchCode,
            DestinationBranchReference = entity.DestinationBranchReference,
            DestinationBranchCode = entity.DestinationBranchCode,
            BranchReference = entity.BranchReference,
            BranchCode = entity.BranchCode,
            Direction = entity.Direction,
            Lines = entity.Lines.Select(ToLineDocument).ToList(),
            CreatedAtUtc = entity.CreatedAtUtc,
            CreatedBy = entity.CreatedBy,
            ModifiedAtUtc = entity.ModifiedAtUtc,
            ModifiedBy = entity.ModifiedBy,
        };
    }

    private static InventoryAdjustmentLine ToLineEntity(InventoryAdjustmentLineDocument document)
    {
        return new InventoryAdjustmentLine
        {
            LineId = document.LineId,
            ItemReference = document.ItemReference,
            ItemCode = document.ItemCode,
            Condition = document.Condition,
            Quantity = document.Quantity,
            Notes = document.Notes,
        };
    }

    private static InventoryAdjustmentLineDocument ToLineDocument(InventoryAdjustmentLine entity)
    {
        return new InventoryAdjustmentLineDocument
        {
            LineId = entity.LineId,
            ItemReference = entity.ItemReference,
            ItemCode = entity.ItemCode,
            Condition = entity.Condition,
            Quantity = entity.Quantity,
            Notes = entity.Notes,
        };
    }
}
