using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Domain.Enums;

namespace HenryTires.Inventory.Application.Ports;

public interface IInventoryAdjustmentRepository
{
    Task<InventoryAdjustment?> GetByIdAsync(string id);
    Task<IEnumerable<InventoryAdjustment>> SearchAsync(
        string? branchReference,
        AdjustmentType? adjustmentType,
        InventoryAdjustmentStatus? status,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize
    );
    Task<int> CountAsync(
        string? branchReference,
        AdjustmentType? adjustmentType,
        InventoryAdjustmentStatus? status,
        DateTime? from,
        DateTime? to
    );
    Task<InventoryAdjustment> CreateAsync(InventoryAdjustment adjustment);
    Task UpdateAsync(InventoryAdjustment adjustment);
}
