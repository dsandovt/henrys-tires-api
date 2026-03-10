using HenryTires.Inventory.Application.Ports.Outbound;
using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Domain.Enums;

namespace HenryTires.Inventory.Application.Ports;

public interface IInventorySummaryRepository
{
    Task<InventorySummary?> GetByKeyAsync(
        string branchReference,
        string itemCode,
        ITransactionScope? transactionScope = null
    );
    Task<IEnumerable<InventorySummary>> GetByBranchAsync(
        string? branchReference,
        string? search,
        ItemCondition? condition,
        int page,
        int pageSize
    );
    Task<long> CountByBranchAsync(string? branchReference, string? search, ItemCondition? condition);
    Task<int> GetTotalQuantityByBranchAsync(string branchReference);

    Task UpsertAsync(InventorySummary summary, ITransactionScope? transactionScope = null);
    Task UpsertWithVersionCheckAsync(InventorySummary summary, ITransactionScope transactionScope);
}
