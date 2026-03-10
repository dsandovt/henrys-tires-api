using HenryTires.Inventory.Domain.Common;
using HenryTires.Inventory.Domain.ValueObjects;

namespace HenryTires.Inventory.Domain.Entities;

public class User : AuditTrail
{
    public required string Id { get; set; }
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public required string FirstName { get; set; }
    public string? MiddleName { get; set; }
    public required string LastName { get; set; }
    public string? SecondLastName { get; set; }
    public string? Email { get; set; }
    public required List<string> GroupReferences { get; set; }
    public required List<string> BranchReferences { get; set; }
    public required bool IsActive { get; set; }

    public UserLite ToUserLite() => new()
    {
        FirstName = FirstName,
        MiddleName = MiddleName,
        LastName = LastName,
        SecondLastName = SecondLastName,
        Username = Username,
        Email = Email,
    };
}
