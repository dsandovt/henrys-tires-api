using HenryTires.Inventory.Application.Ports.Outbound;
using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Domain.Enums;

namespace HenryTires.Inventory.Application.Ports;

public interface IInventoryTransactionRepository
{
    Task<InventoryTransaction?> GetByIdAsync(string id);

    Task<IEnumerable<InventoryTransaction>> SearchAsync(
        string? branchReference,
        DateTime? from,
        DateTime? to,
        InitiatorType? initiatorType,
        InventoryTransactionStatus? status,
        string? itemCode,
        ItemCondition? condition,
        int page,
        int pageSize
    );
    Task<long> CountAsync(
        string? branchReference,
        DateTime? from,
        DateTime? to,
        InitiatorType? initiatorType,
        InventoryTransactionStatus? status,
        string? itemCode,
        ItemCondition? condition
    );

    Task<InventoryTransaction> CreateAsync(InventoryTransaction transaction);
    Task UpdateAsync(InventoryTransaction transaction, ITransactionScope? transactionScope = null);
}
