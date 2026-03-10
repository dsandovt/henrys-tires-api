using HenryTires.Inventory.Domain.Common;

namespace HenryTires.Inventory.Domain.Entities;

public class Group : AuditTrail
{
    public required string Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required List<string> RoleReferences { get; set; }
    public required bool IsActive { get; set; }
}
