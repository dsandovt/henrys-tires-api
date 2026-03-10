namespace HenryTires.Inventory.Application.Ports;

public interface ICurrentUserService
{
    string? Username { get; }
    string? UserId { get; }
    IReadOnlyList<string>? GroupReferences { get; }
    IReadOnlyList<string>? RoleCodes { get; }
    IReadOnlyList<string>? BranchReferences { get; }
    IReadOnlyList<string>? BranchCodes { get; }
}
