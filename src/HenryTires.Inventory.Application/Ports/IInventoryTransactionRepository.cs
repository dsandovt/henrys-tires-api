using HenryTires.Inventory.Application.Ports.Outbound;
using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Domain.Enums;

namespace HenryTires.Inventory.Application.Ports;

public interface IInventoryTransactionRepository
{
    Task<InventoryTransaction?> GetByIdAsync(string id);

    Task<InventoryTransaction?> GetByTransactionNumberAsync(string transactionNumber);
    Task<IEnumerable<InventoryTransaction>> SearchAsync(
        string? branchCode,
        DateTime? from,
        DateTime? to,
        TransactionType? type,
        TransactionStatus? status,
        string? itemCode,
        ItemCondition? condition,
        int page,
        int pageSize
    );
    Task<long> CountAsync(
        string? branchCode,
        DateTime? from,
        DateTime? to,
        TransactionType? type,
        TransactionStatus? status,
        string? itemCode,
        ItemCondition? condition
    );

    // Custom command methods
    Task<InventoryTransaction> CreateAsync(InventoryTransaction transaction);
    Task UpdateAsync(InventoryTransaction transaction, ITransactionScope? transactionScope = null);
}
