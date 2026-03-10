using HenryTires.Inventory.Domain.Common;
using HenryTires.Inventory.Domain.Enums;
using HenryTires.Inventory.Domain.ValueObjects;

namespace HenryTires.Inventory.Domain.Entities;

public class Sale : AuditTrail
{
    public required string Id { get; set; }
    public required string Number { get; set; }
    public required string BranchReference { get; set; }
    public required string BranchCode { get; set; }
    public required DateTime SaleDateUtc { get; set; }
    public required List<SaleLine> Lines { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? Notes { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    public List<PaymentDetail>? PaymentDetails { get; set; }
    public SaleStatus Status { get; set; }
    public required List<StatusHistoryEntry<SaleStatus>> StatusHistory { get; set; }
}
