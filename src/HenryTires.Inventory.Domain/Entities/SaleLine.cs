using HenryTires.Inventory.Domain.Enums;

namespace HenryTires.Inventory.Domain.Entities;

public class SaleLine
{
    public string? LineId { get; set; } // Optional line identifier
    public required string ItemId { get; set; }
    public required string ItemCode { get; set; }
    public required string Description { get; set; }
    public required Classification Classification { get; set; }
    public ItemCondition? Condition { get; set; } // Only for Goods
    public required int Quantity { get; set; }
    public required decimal UnitPrice { get; set; }
    public required string Currency { get; set; }
    public decimal LineTotal => Quantity * UnitPrice;
    public string? InventoryTransactionId { get; set; } // Set if Classification = Good
}
