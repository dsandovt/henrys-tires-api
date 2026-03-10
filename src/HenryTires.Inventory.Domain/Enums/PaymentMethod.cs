using System.Text.Json.Serialization;

namespace HenryTires.Inventory.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PaymentMethod
{
    Cash,
    Card,
    AcimaShortTermCredit,
    AccountsReceivable,
    Check,
    Transfer,
    Split,
}
