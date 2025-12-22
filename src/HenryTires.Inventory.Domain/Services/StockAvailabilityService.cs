using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Domain.Enums;

namespace HenryTires.Inventory.Domain.Services;

/// <summary>
/// Domain service for checking stock availability
/// </summary>
public class StockAvailabilityService
{
    public StockCheckResult CheckAvailability(
        InventorySummary? summary,
        ItemCondition condition,
        int requestedQuantity)
    {
        if (summary == null)
        {
            return StockCheckResult.Insufficient(0, requestedQuantity);
        }

        var entry = summary.Entries.FirstOrDefault(e => e.Condition == condition);
        if (entry == null)
        {
            return StockCheckResult.Insufficient(0, requestedQuantity);
        }

        int available = entry.OnHand - entry.Reserved;

        if (available >= requestedQuantity)
        {
            return StockCheckResult.Sufficient(available);
        }
        else
        {
            return StockCheckResult.Insufficient(available, requestedQuantity);
        }
    }
}

/// <summary>
/// Result of stock availability check
/// </summary>
public class StockCheckResult
{
    public bool IsSufficient { get; private set; }
    public int Available { get; private set; }
    public int Requested { get; private set; }

    private StockCheckResult(bool isSufficient, int available, int requested)
    {
        IsSufficient = isSufficient;
        Available = available;
        Requested = requested;
    }

    public static StockCheckResult Sufficient(int available)
    {
        return new StockCheckResult(true, available, 0);
    }

    public static StockCheckResult Insufficient(int available, int requested)
    {
        return new StockCheckResult(false, available, requested);
    }
}
