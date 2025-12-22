using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Domain.Enums;
using MongoDB.Driver;

namespace HenryTires.Inventory.Application.Ports;

/// <summary>
/// Inventory summary repository interface. Basic CRUD operations are inherited from CrudRepository.
/// This interface defines domain-specific queries and commands for inventory summaries.
/// </summary>
public interface IInventorySummaryRepository
{
    // Custom query methods
    Task<InventorySummary?> GetByKeyAsync(string branchCode, string itemCode, IClientSessionHandle? session = null);
    Task<IEnumerable<InventorySummary>> GetByBranchAsync(string? branchCode, string? search, ItemCondition? condition, int page, int pageSize);
    Task<long> CountByBranchAsync(string? branchCode, string? search, ItemCondition? condition);
    Task<int> GetTotalQuantityByBranchAsync(string branchCode);

    // Custom command methods with transaction support
    Task UpsertAsync(InventorySummary summary, IClientSessionHandle? session = null);
    Task UpsertWithVersionCheckAsync(InventorySummary summary, IClientSessionHandle session);
}
