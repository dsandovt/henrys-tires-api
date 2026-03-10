using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Documents;

namespace HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Mappings;

public static class PurchaseOrderDocumentMapper
{
    public static PurchaseOrder ToEntity(PurchaseOrderDocument document)
    {
        return new PurchaseOrder
        {
            Id = document.Id,
            Number = document.Number,
            BranchReference = document.BranchReference,
            BranchCode = document.BranchCode,
            OrderDateUtc = document.OrderDateUtc,
            Lines = document.Lines.Select(ToLineEntity).ToList(),
            SupplierName = document.SupplierName,
            Notes = document.Notes,
            Status = document.Status,
            StatusHistory = document.StatusHistory.Select(StatusHistoryMapper.ToEntity).ToList(),
            CreatedAtUtc = document.CreatedAtUtc,
            CreatedBy = document.CreatedBy,
            ModifiedAtUtc = document.ModifiedAtUtc,
            ModifiedBy = document.ModifiedBy,
        };
    }

    public static PurchaseOrderDocument ToDocument(PurchaseOrder entity)
    {
        return new PurchaseOrderDocument
        {
            Id = entity.Id,
            Number = entity.Number,
            BranchReference = entity.BranchReference,
            BranchCode = entity.BranchCode,
            OrderDateUtc = entity.OrderDateUtc,
            Lines = entity.Lines.Select(ToLineDocument).ToList(),
            SupplierName = entity.SupplierName,
            Notes = entity.Notes,
            Status = entity.Status,
            StatusHistory = entity.StatusHistory.Select(StatusHistoryMapper.ToDocument).ToList(),
            CreatedAtUtc = entity.CreatedAtUtc,
            CreatedBy = entity.CreatedBy,
            ModifiedAtUtc = entity.ModifiedAtUtc,
            ModifiedBy = entity.ModifiedBy,
        };
    }

    private static PurchaseOrderLine ToLineEntity(PurchaseOrderLineDocument document)
    {
        return new PurchaseOrderLine
        {
            LineId = document.LineId,
            ItemReference = document.ItemReference,
            ItemCode = document.ItemCode,
            Condition = document.Condition,
            Quantity = document.Quantity,
            UnitPrice = document.UnitPrice,
            Currency = document.Currency,
        };
    }

    private static PurchaseOrderLineDocument ToLineDocument(PurchaseOrderLine entity)
    {
        return new PurchaseOrderLineDocument
        {
            LineId = entity.LineId,
            ItemReference = entity.ItemReference,
            ItemCode = entity.ItemCode,
            Condition = entity.Condition,
            Quantity = entity.Quantity,
            UnitPrice = entity.UnitPrice,
            Currency = entity.Currency,
        };
    }
}
