using HenryTires.Inventory.Domain.Common;
using HenryTires.Inventory.Domain.Enums;
using HenryTires.Inventory.Domain.ValueObjects;

namespace HenryTires.Inventory.Domain.Entities;

public class PurchaseOrder : AuditTrail
{
    public required string Id { get; set; }
    public required string Number { get; set; }
    public required string BranchReference { get; set; }
    public required string BranchCode { get; set; }
    public required DateTime OrderDateUtc { get; set; }
    public required List<PurchaseOrderLine> Lines { get; set; }
    public string? SupplierName { get; set; }
    public string? Notes { get; set; }
    public required PurchaseOrderStatus Status { get; set; }
    public required List<StatusHistoryEntry<PurchaseOrderStatus>> StatusHistory { get; set; }
}

public class PurchaseOrderLine
{
    public required string LineId { get; set; }
    public required string ItemReference { get; set; }
    public required string ItemCode { get; set; }
    public required ItemCondition Condition { get; set; }
    public required int Quantity { get; set; }
    public required decimal UnitPrice { get; set; }
    public required Currency Currency { get; set; }
    public decimal LineTotal => Quantity * UnitPrice;
}
