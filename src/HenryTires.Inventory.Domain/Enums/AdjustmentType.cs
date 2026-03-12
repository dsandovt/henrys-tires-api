using System.Text.Json.Serialization;

namespace HenryTires.Inventory.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AdjustmentType
{
    BranchTransfer = 1,
    StockCorrection = 2,
}
