using HenryTires.Inventory.Domain.Common;
using HenryTires.Inventory.Domain.Enums;

namespace HenryTires.Inventory.Domain.Entities;

public class ConsumableItemPrice : AuditTrail
{
    public required string Id { get; set; }
    public required string ItemCode { get; set; }
    public required Currency Currency { get; set; }
    public required decimal LatestPrice { get; set; }
    public required DateTime LatestPriceDateUtc { get; set; }
    public required List<PriceHistoryEntry> History { get; set; }

    public void UpdatePrice(decimal newPrice, string updatedBy, DateTime dateUtc)
    {
        if (newPrice < 0)
            throw new ArgumentException("Price must be greater than zero", nameof(newPrice));

        History.Add(
            new PriceHistoryEntry
            {
                Price = LatestPrice,
                DateUtc = LatestPriceDateUtc,
                UpdatedBy = ModifiedBy ?? CreatedBy,
            }
        );

        LatestPrice = newPrice;
        LatestPriceDateUtc = dateUtc;
        ModifiedBy = updatedBy;
    }
}

public class PriceHistoryEntry
{
    public required decimal Price { get; set; }
    public required DateTime DateUtc { get; set; }
    public required string UpdatedBy { get; set; }
}
