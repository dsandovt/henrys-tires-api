using HenryTires.Inventory.Application.Ports.Outbound;
using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Domain.Enums;

namespace HenryTires.Inventory.Application.Ports;

/// <summary>
/// Inventory transaction repository interface. Implementation inherits from CrudRepository.
/// This interface defines transaction-specific queries and commands.
/// </summary>
public interface IInventoryTransactionRepository
{
    // Basic CRUD operations (implemented by CrudRepository)
    Task<InventoryTransaction?> GetByIdAsync(string id);

    // Custom query methods
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
        int pageSize);
    Task<long> CountAsync(
        string? branchCode,
        DateTime? from,
        DateTime? to,
        TransactionType? type,
        TransactionStatus? status,
        string? itemCode,
        ItemCondition? condition);

    // Custom command methods
    Task<InventoryTransaction> CreateAsync(InventoryTransaction transaction);
    Task UpdateAsync(InventoryTransaction transaction, ITransactionScope? transactionScope = null);
}
