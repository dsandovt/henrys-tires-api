using System.Text.Json.Serialization;

namespace HenryTires.Inventory.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum InventoryTransactionStatus
{
    Draft = 0,
    Committed = 1,
    Cancelled = 2,
}
