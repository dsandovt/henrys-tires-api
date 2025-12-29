using HenryTires.Inventory.Domain.Common;
using HenryTires.Inventory.Domain.Enums;

namespace HenryTires.Inventory.Domain.Entities;

public class InventoryTransaction : AuditTrail
{
    public required string Id { get; set; }
    public required string TransactionNumber { get; set; }
    public required string BranchCode { get; set; }
    public required TransactionType Type { get; set; }
    public required TransactionStatus Status { get; set; }
    public required DateTime TransactionDateUtc { get; set; }
    public string? Notes { get; set; }
    public DateTime? CommittedAtUtc { get; set; }
    public string? CommittedBy { get; set; }
    public required List<InventoryTransactionLine> Lines { get; set; }

    public void Commit(string committedBy, DateTime committedAtUtc)
    {
        if (Status != TransactionStatus.Draft)
            throw new InvalidOperationException(
                $"Cannot commit transaction with status {Status}. Only Draft transactions can be committed."
            );

        Status = TransactionStatus.Committed;
        CommittedAtUtc = committedAtUtc;
        CommittedBy = committedBy;
        ModifiedAtUtc = committedAtUtc;
        ModifiedBy = committedBy;
    }

    public void Cancel()
    {
        if (Status == TransactionStatus.Committed)
            throw new InvalidOperationException(
                "Cannot cancel a committed transaction. Create a reversal transaction instead."
            );

        if (Status == TransactionStatus.Cancelled)
            throw new InvalidOperationException("Transaction is already cancelled.");

        Status = TransactionStatus.Cancelled;
    }

    public decimal GetTotalAmount()
    {
        return Lines.Sum(l => l.LineTotal);
    }
}
