using HenryTires.Inventory.Domain.Common;
using HenryTires.Inventory.Domain.Enums;
using HenryTires.Inventory.Domain.ValueObjects;

namespace HenryTires.Inventory.Domain.Entities;

public class InventoryAdjustment : AuditTrail
{
    public required string Id { get; set; }
    public required string Number { get; set; }
    public required AdjustmentType AdjustmentType { get; set; }
    public required InventoryAdjustmentStatus Status { get; set; }
    public required List<StatusHistoryEntry<InventoryAdjustmentStatus>> StatusHistory { get; set; }
    public required DateTime AdjustmentDateUtc { get; set; }
    public string? Notes { get; set; }

    // BranchTransfer only
    public string? OriginBranchReference { get; set; }
    public string? OriginBranchCode { get; set; }
    public string? DestinationBranchReference { get; set; }
    public string? DestinationBranchCode { get; set; }

    // StockCorrection only
    public string? BranchReference { get; set; }
    public string? BranchCode { get; set; }
    public CorrectionDirection? Direction { get; set; }

    public required List<InventoryAdjustmentLine> Lines { get; set; }
}

public class InventoryAdjustmentLine
{
    public required string LineId { get; set; }
    public required string ItemReference { get; set; }
    public required string ItemCode { get; set; }
    public required ItemCondition Condition { get; set; }
    public required int Quantity { get; set; }
    public string? Notes { get; set; }
}
