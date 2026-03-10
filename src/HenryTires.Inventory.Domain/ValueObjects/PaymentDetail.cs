using HenryTires.Inventory.Domain.Enums;

namespace HenryTires.Inventory.Domain.ValueObjects;

public class PaymentDetail
{
    public PaymentMethod Method { get; set; }
    public decimal Amount { get; set; }
    public string? CheckNumber { get; set; }
}
