using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Domain.Enums;
using MongoDB.Driver;

namespace HenryTires.Inventory.Application.Ports;

/// <summary>
/// Inventory balance repository interface. Basic CRUD operations are inherited from CrudRepository.
/// This interface defines balance-specific queries and commands.
/// </summary>
public interface IInventoryBalanceRepository
{
    // Custom query methods
    Task<InventoryBalance?> GetByKeyAsync(string branchId, string itemId, ItemCondition condition);
    Task<IEnumerable<InventoryBalance>> GetByBranchAsync(string branchId, string? search, ItemCondition? condition, int page, int pageSize);
    Task<long> CountByBranchAsync(string branchId, string? search, ItemCondition? condition);
    Task<int> GetTotalQuantityByBranchAsync(string branchId);
    Task<IEnumerable<InventoryBalance>> GetAllBranchesStockAsync(string? search, ItemCondition? condition);

    // Custom command methods
    Task UpsertAsync(InventoryBalance balance, IClientSessionHandle? session = null);
}
