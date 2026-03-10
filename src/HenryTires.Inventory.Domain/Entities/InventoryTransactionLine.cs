using HenryTires.Inventory.Domain.Enums;

namespace HenryTires.Inventory.Domain.Entities;

public class InventoryTransactionLine
{
    public required string LineId { get; set; }
    public required string ItemCode { get; set; }
    public required ItemCondition Condition { get; set; }
    public required int Quantity { get; set; }
}
