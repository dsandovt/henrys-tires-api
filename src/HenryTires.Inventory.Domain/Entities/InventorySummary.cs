using HenryTires.Inventory.Domain.Enums;

namespace HenryTires.Inventory.Domain.Entities;

public class InventorySummary
{
    public required string Id { get; set; }
    public required string BranchReference { get; set; }
    public required string BranchCode { get; set; }
    public required string ItemCode { get; set; }
    public required List<InventoryEntry> Entries { get; set; }
    public required int OnHandTotal { get; set; }
    public required int ReservedTotal { get; set; }
    public required int Version { get; set; }
    public required DateTime UpdatedAtUtc { get; set; }

    public void IncreaseStock(string itemCode, ItemCondition condition, int quantity, DateTime date)
    {
        var entry = GetOrCreateEntry(condition, date);
        entry.OnHand += quantity;
        entry.LatestEntryDateUtc = date;
        RecalculateTotals();
    }

    public void DecreaseStock(string itemCode, ItemCondition condition, int quantity, DateTime date)
    {
        var entry = GetOrCreateEntry(condition, date);
        if (entry.OnHand - quantity < 0 && condition != ItemCondition.New)
            throw new InvalidOperationException(
                $"Insufficient stock for {itemCode} ({condition})."
            );
        entry.OnHand -= quantity;
        entry.LatestEntryDateUtc = date;
        RecalculateTotals();
    }

    public void OverrideStock(string itemCode, ItemCondition condition, int quantity, DateTime date)
    {
        var entry = GetOrCreateEntry(condition, date);
        entry.OnHand = quantity;
        entry.LatestEntryDateUtc = date;
        RecalculateTotals();
    }

    public int GetAvailable(ItemCondition condition)
    {
        var entry = Entries.FirstOrDefault(e => e.Condition == condition);
        return entry != null ? entry.OnHand - entry.Reserved : 0;
    }

    private InventoryEntry GetOrCreateEntry(ItemCondition condition, DateTime date)
    {
        var entry = Entries.FirstOrDefault(e => e.Condition == condition);

        if (entry == null)
        {
            entry = new InventoryEntry
            {
                Condition = condition,
                OnHand = 0,
                Reserved = 0,
                LatestEntryDateUtc = date,
            };
            Entries.Add(entry);
        }

        return entry;
    }

    private void RecalculateTotals()
    {
        OnHandTotal = Entries.Sum(e => e.OnHand);
        ReservedTotal = Entries.Sum(e => e.Reserved);
        Version++;
    }
}

public class InventoryEntry
{
    public required ItemCondition Condition { get; set; }
    public required int OnHand { get; set; }
    public required int Reserved { get; set; }
    public required DateTime LatestEntryDateUtc { get; set; }
}
