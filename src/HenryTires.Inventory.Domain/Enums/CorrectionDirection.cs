using System.Text.Json.Serialization;

namespace HenryTires.Inventory.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CorrectionDirection
{
    Increase = 1,
    Decrease = 2,
}
