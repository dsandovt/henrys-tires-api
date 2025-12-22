using HenryTires.Inventory.Domain.Enums;

namespace HenryTires.Inventory.Application.Ports;

public interface ICurrentUser
{
    string UserId { get; }
    string Username { get; }
    Role Role { get; }
    string? BranchId { get; } // Legacy - maps to Branch.Id
    string? BranchCode { get; } // New schema - maps to Branch.Code
}
