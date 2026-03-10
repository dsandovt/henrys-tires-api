using HenryTires.Inventory.Domain.Common;
using HenryTires.Inventory.Domain.Enums;
using HenryTires.Inventory.Domain.ValueObjects;

namespace HenryTires.Inventory.Domain.Entities;

public class InventoryTransaction : AuditTrail
{
    public required string Id { get; set; }
    public required string BranchReference { get; set; }
    public required string BranchCode { get; set; }
    public required EntityKey Initiator { get; set; }
    public required InventoryTransactionStatus Status { get; set; }
    public required DateTime TransactionDateUtc { get; set; }
    public string? Notes { get; set; }
    public required List<InventoryTransactionLine> Lines { get; set; }
    public required List<StatusHistoryEntry<InventoryTransactionStatus>> StatusHistory { get; set; }

    public void Commit(UserLite user, DateTime date, string? comment = null)
    {
        if (Status != InventoryTransactionStatus.Draft)
            throw new InvalidOperationException(
                $"Cannot commit transaction with status {Status}. Only Draft transactions can be committed."
            );

        Status = InventoryTransactionStatus.Committed;
        StatusHistory.Add(new StatusHistoryEntry<InventoryTransactionStatus>
        {
            Date = date,
            Status = InventoryTransactionStatus.Committed,
            User = user,
            Comment = comment,
        });
        ModifiedAtUtc = date;
        ModifiedBy = user.Username;
    }

    public void Cancel(UserLite user, DateTime date, string? comment = null)
    {
        if (Status == InventoryTransactionStatus.Committed)
            throw new InvalidOperationException(
                "Cannot cancel a committed transaction. Create a reversal transaction instead."
            );

        if (Status == InventoryTransactionStatus.Cancelled)
            throw new InvalidOperationException("Transaction is already cancelled.");

        Status = InventoryTransactionStatus.Cancelled;
        StatusHistory.Add(new StatusHistoryEntry<InventoryTransactionStatus>
        {
            Date = date,
            Status = InventoryTransactionStatus.Cancelled,
            User = user,
            Comment = comment,
        });
        ModifiedAtUtc = date;
        ModifiedBy = user.Username;
    }
}
