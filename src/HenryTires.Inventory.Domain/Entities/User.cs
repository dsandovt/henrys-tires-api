using HenryTires.Inventory.Domain.Common;
using HenryTires.Inventory.Domain.Enums;

namespace HenryTires.Inventory.Domain.Entities;

public class User : AuditTrail
{
    public required string Id { get; set; }
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public required Role Role { get; set; }
    public string? BranchId { get; set; }

    public required bool IsActive { get; set; }
}
