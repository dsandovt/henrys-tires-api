using System.Text.Json.Serialization;

namespace HenryTires.Inventory.Domain.Enums;

/// <summary>
/// Payment method for sales and inventory transactions
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PaymentMethod
{
    Cash,
    Card,
    AcimaShortTermCredit,
    AccountsReceivable
}
