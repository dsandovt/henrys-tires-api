using HenryTires.Inventory.Domain.Enums;

namespace HenryTires.Inventory.Domain.Entities;

public class InventorySummary
{
    public required string Id { get; set; }
    public required string BranchCode { get; set; }
    public required string ItemCode { get; set; }
    public required List<InventoryEntry> Entries { get; set; }
    public required int OnHandTotal { get; set; }
    public required int ReservedTotal { get; set; }
    public required int Version { get; set; }
    public required DateTime UpdatedAtUtc { get; set; }

    public void ApplyTransaction(InventoryTransaction transaction)
    {
        var relevantLines = transaction.Lines.Where(l => l.ItemCode == ItemCode).ToList();

        foreach (var line in relevantLines)
        {
            var entry = Entries.FirstOrDefault(e => e.Condition == line.Condition);

            if (entry == null)
            {
                entry = new InventoryEntry
                {
                    Condition = line.Condition,
                    OnHand = 0,
                    Reserved = 0,
                    LatestEntryDateUtc = transaction.TransactionDateUtc,
                };
                Entries.Add(entry);
            }

            switch (transaction.Type)
            {
                case TransactionType.In:
                    entry.OnHand += line.Quantity;
                    break;
                case TransactionType.Out:
                    entry.OnHand -= line.Quantity;
                    if (entry.OnHand < 0)
                        throw new InvalidOperationException(
                            $"Stock cannot be negative for {ItemCode} ({line.Condition}). "
                                + $"Attempted: {entry.OnHand}"
                        );
                    break;
                case TransactionType.Adjust:
                    entry.OnHand = line.Quantity;
                    break;
            }

            entry.LatestEntryDateUtc = transaction.TransactionDateUtc;
        }

        OnHandTotal = Entries.Sum(e => e.OnHand);
        ReservedTotal = Entries.Sum(e => e.Reserved);
        Version++;
    }

    public int GetAvailable(ItemCondition condition)
    {
        var entry = Entries.FirstOrDefault(e => e.Condition == condition);
        return entry != null ? entry.OnHand - entry.Reserved : 0;
    }
}

public class InventoryEntry
{
    public required ItemCondition Condition { get; set; }
    public required int OnHand { get; set; }
    public required int Reserved { get; set; }
    public required DateTime LatestEntryDateUtc { get; set; }
}
