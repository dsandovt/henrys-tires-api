using HenryTires.Inventory.Domain.ValueObjects;

namespace HenryTires.Inventory.Application.Ports;

public interface ICurrentUser
{
    string UserId { get; }
    string Username { get; }
    string FirstName { get; }
    string LastName { get; }
    string? MiddleName { get; }
    string? SecondLastName { get; }
    string? Email { get; }
    IReadOnlyList<string> GroupReferences { get; }
    IReadOnlyList<string> RoleCodes { get; }
    IReadOnlyList<string> BranchReferences { get; }
    IReadOnlyList<string> BranchCodes { get; }
    bool HasRole(string roleCode);
    bool CanAccessBranch(string branchReference);
    bool CanAccessBranchCode(string branchCode);
    UserLite ToUserLite();
}
