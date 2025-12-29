using HenryTires.Inventory.Domain.Enums;

namespace HenryTires.Inventory.Application.Ports;

public interface ICurrentUserService
{
    string? Username { get; }
    string? UserId { get; }
    Role? UserRole { get; }
    string? BranchId { get; }
    string? BranchCode { get; }
}
