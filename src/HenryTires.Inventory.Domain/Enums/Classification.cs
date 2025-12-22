using System.Text.Json.Serialization;

namespace HenryTires.Inventory.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Classification
{
    Good = 0,
    Service = 1,
}
