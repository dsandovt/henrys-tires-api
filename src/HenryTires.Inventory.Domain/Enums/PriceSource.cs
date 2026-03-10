using System.Text.Json.Serialization;

namespace HenryTires.Inventory.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PriceSource
{
    ConsumableItemPrice = 0,
    Manual = 1,
    Sale = 2,
    SystemDefault = 3,
    PurchaseOrder = 4,
    AverageCost = 5,
}
