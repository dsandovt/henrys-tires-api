using System.Text.Json.Serialization;

namespace HenryTires.Inventory.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ItemCondition
{
    New = 0,
    Used = 1,
}
