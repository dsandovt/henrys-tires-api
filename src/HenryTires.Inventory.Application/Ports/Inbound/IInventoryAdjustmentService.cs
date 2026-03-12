using HenryTires.Inventory.Application.Common;
using HenryTires.Inventory.Application.DTOs;
using HenryTires.Inventory.Domain.Enums;

namespace HenryTires.Inventory.Application.Ports.Inbound;

public interface IInventoryAdjustmentService
{
    Task<InventoryAdjustmentDto> CreateBranchTransferAsync(CreateBranchTransferDto request);
    Task<InventoryAdjustmentDto> CreateStockCorrectionAsync(CreateStockCorrectionDto request);
    Task<InventoryAdjustmentDto> CommitAdjustmentAsync(string id);
    Task<InventoryAdjustmentDto> CancelAdjustmentAsync(string id);
    Task<InventoryAdjustmentDto> GetByIdAsync(string id);
    Task<PaginatedResponse<InventoryAdjustmentDto>> SearchAsync(
        string? branchReference,
        AdjustmentType? adjustmentType,
        InventoryAdjustmentStatus? status,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize
    );
}
