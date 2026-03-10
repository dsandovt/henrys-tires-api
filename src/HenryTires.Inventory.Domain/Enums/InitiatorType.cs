using System.Text.Json.Serialization;

namespace HenryTires.Inventory.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum InitiatorType
{
    PurchaseOrder = 1,
    Sale = 2,
    StockAdjustment = 3,
    StockLoss = 4,
    BranchTransfer = 5,
}
