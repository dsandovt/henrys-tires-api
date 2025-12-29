using System.Text.Json.Serialization;

namespace HenryTires.Inventory.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Currency
{
    USD = 0,
    DOP = 1,
}
