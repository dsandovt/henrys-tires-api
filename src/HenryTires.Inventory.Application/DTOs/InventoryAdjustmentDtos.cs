using HenryTires.Inventory.Domain.Enums;

namespace HenryTires.Inventory.Application.DTOs;

public class InventoryAdjustmentDto
{
    public required string Id { get; set; }
    public required string Number { get; set; }
    public required string AdjustmentType { get; set; }
    public required string Status { get; set; }
    public required List<StatusHistoryEntryDto> StatusHistory { get; set; }
    public required DateTime AdjustmentDateUtc { get; set; }
    public string? Notes { get; set; }

    // BranchTransfer
    public string? OriginBranchReference { get; set; }
    public string? OriginBranchCode { get; set; }
    public string? DestinationBranchReference { get; set; }
    public string? DestinationBranchCode { get; set; }

    // StockCorrection
    public string? BranchReference { get; set; }
    public string? BranchCode { get; set; }
    public string? Direction { get; set; }

    public required List<InventoryAdjustmentLineDto> Lines { get; set; }
    public required DateTime CreatedAtUtc { get; set; }
    public required string CreatedBy { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }
    public string? ModifiedBy { get; set; }
}

public class InventoryAdjustmentLineDto
{
    public required string LineId { get; set; }
    public required string ItemReference { get; set; }
    public required string ItemCode { get; set; }
    public required string Condition { get; set; }
    public required int Quantity { get; set; }
    public string? Notes { get; set; }
}

public class CreateBranchTransferDto
{
    public required string OriginBranchCode { get; set; }
    public required string DestinationBranchCode { get; set; }
    public string? Notes { get; set; }
    public required List<CreateAdjustmentLineDto> Lines { get; set; }
}

public class CreateStockCorrectionDto
{
    public string? BranchCode { get; set; }
    public required CorrectionDirection Direction { get; set; }
    public string? Notes { get; set; }
    public required List<CreateAdjustmentLineDto> Lines { get; set; }
}

public class CreateAdjustmentLineDto
{
    public required string ItemReference { get; set; }
    public required string ItemCode { get; set; }
    public required string Condition { get; set; }
    public required int Quantity { get; set; }
    public string? Notes { get; set; }
}
