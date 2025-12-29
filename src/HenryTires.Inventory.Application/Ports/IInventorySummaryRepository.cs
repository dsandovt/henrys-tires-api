using HenryTires.Inventory.Application.Ports.Outbound;
using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Domain.Enums;

namespace HenryTires.Inventory.Application.Ports;

public interface IInventorySummaryRepository
{
    Task<InventorySummary?> GetByKeyAsync(
        string branchCode,
        string itemCode,
        ITransactionScope? transactionScope = null
    );
    Task<IEnumerable<InventorySummary>> GetByBranchAsync(
        string? branchCode,
        string? search,
        ItemCondition? condition,
        int page,
        int pageSize
    );
    Task<long> CountByBranchAsync(string? branchCode, string? search, ItemCondition? condition);
    Task<int> GetTotalQuantityByBranchAsync(string branchCode);

    Task UpsertAsync(InventorySummary summary, ITransactionScope? transactionScope = null);
    Task UpsertWithVersionCheckAsync(InventorySummary summary, ITransactionScope transactionScope);
}
