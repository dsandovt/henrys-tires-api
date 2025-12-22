namespace HenryTires.Inventory.Domain.Common;

public abstract class AuditTrail
{
    public required DateTime CreatedAtUtc { get; set; }
    public required string CreatedBy { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }
    public string? ModifiedBy { get; set; }
}
