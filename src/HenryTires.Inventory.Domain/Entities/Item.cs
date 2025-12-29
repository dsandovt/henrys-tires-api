using HenryTires.Inventory.Domain.Common;
using HenryTires.Inventory.Domain.Enums;

namespace HenryTires.Inventory.Domain.Entities;

public class Item : AuditTrail
{
    public required string Id { get; set; }
    public required string ItemCode { get; set; }
    public required string Description { get; set; }
    public required Classification Classification { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }
}
