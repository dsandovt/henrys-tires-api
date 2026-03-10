using HenryTires.Inventory.Domain.Enums;

namespace HenryTires.Inventory.Application.DTOs;

public class PurchaseOrderDto
{
    public required string Id { get; set; }
    public required string Number { get; set; }
    public required string BranchReference { get; set; }
    public required string BranchCode { get; set; }
    public required DateTime OrderDateUtc { get; set; }
    public required List<PurchaseOrderLineDto> Lines { get; set; }
    public string? SupplierName { get; set; }
    public string? Notes { get; set; }
    public required string Status { get; set; }
    public required List<StatusHistoryEntryDto> StatusHistory { get; set; }
    public required DateTime CreatedAtUtc { get; set; }
    public required string CreatedBy { get; set; }
}

public class PurchaseOrderLineDto
{
    public required string LineId { get; set; }
    public required string ItemReference { get; set; }
    public required string ItemCode { get; set; }
    public required string Condition { get; set; }
    public required int Quantity { get; set; }
    public required decimal UnitPrice { get; set; }
    public required string Currency { get; set; }
    public required decimal LineTotal { get; set; }
}

public class CreatePurchaseOrderDto
{
    public string? BranchCode { get; set; }
    public DateTime? OrderDateUtc { get; set; }
    public required List<CreatePurchaseOrderLineDto> Lines { get; set; }
    public string? SupplierName { get; set; }
    public string? Notes { get; set; }
}

public class CreatePurchaseOrderLineDto
{
    public required string ItemReference { get; set; }
    public required string ItemCode { get; set; }
    public required string Condition { get; set; }
    public required int Quantity { get; set; }
    public required decimal UnitPrice { get; set; }
    public Currency Currency { get; set; } = Currency.USD;
}