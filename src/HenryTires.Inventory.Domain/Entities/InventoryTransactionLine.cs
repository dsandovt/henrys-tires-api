using HenryTires.Inventory.Domain.Enums;

namespace HenryTires.Inventory.Domain.Entities;

public class InventoryTransactionLine
{
    public required string LineId { get; set; }
    public required string ItemId { get; set; }
    public required string ItemCode { get; set; }
    public required ItemCondition Condition { get; set; }
    public required int Quantity { get; set; }
    public required decimal UnitPrice { get; set; }
    public required Currency Currency { get; set; }
    public bool IsTaxable { get; set; } = true; // Determines if line is included in sales tax calculation
    public bool AppliesShopFee { get; set; } = true; // Determines if line is included in shop fee calculation
    public required PriceSource PriceSource { get; set; }
    public required string PriceSetByRole { get; set; }
    public required string PriceSetByUser { get; set; }
    public required decimal LineTotal { get; set; }
    public decimal? CostOfGoodsSold { get; set; }
    public string? PriceNotes { get; set; }
    public required DateTime ExecutedAtUtc { get; set; }

    public static decimal CalculateLineTotal(int quantity, decimal unitPrice)
    {
        return quantity * unitPrice;
    }
}
