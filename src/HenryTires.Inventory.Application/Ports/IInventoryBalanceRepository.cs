using HenryTires.Inventory.Application.Ports.Outbound;
using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Domain.Enums;

namespace HenryTires.Inventory.Application.Ports;

public interface IInventoryBalanceRepository
{
    Task<InventoryBalance?> GetByKeyAsync(string branchId, string itemId, ItemCondition condition);
    Task<IEnumerable<InventoryBalance>> GetByBranchAsync(
        string branchId,
        string? search,
        ItemCondition? condition,
        int page,
        int pageSize
    );
    Task<long> CountByBranchAsync(string branchId, string? search, ItemCondition? condition);
    Task<int> GetTotalQuantityByBranchAsync(string branchId);
    Task<IEnumerable<InventoryBalance>> GetAllBranchesStockAsync(
        string? search,
        ItemCondition? condition
    );

    Task UpsertAsync(InventoryBalance balance, ITransactionScope? transactionScope = null);
}
