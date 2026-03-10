using HenryTires.Inventory.Domain.Enums;

namespace HenryTires.Inventory.Domain.Entities;

public class SaleLine
{
    public string? LineId { get; set; }
    public required string ItemReference { get; set; }
    public required string ItemCode { get; set; }
    public required string Description { get; set; }
    public required Classification Classification { get; set; }
    public ItemCondition? Condition { get; set; }
    public required int Quantity { get; set; }
    public required decimal UnitPrice { get; set; }
    public required Currency Currency { get; set; }
    public bool IsTaxable { get; set; } = true;
    public bool AppliesShopFee { get; set; } = true;
    public decimal LineTotal => Quantity * UnitPrice;
}
