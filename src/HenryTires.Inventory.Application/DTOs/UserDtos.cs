namespace HenryTires.Inventory.Application.DTOs;

public class UserDto
{
    public required string Id { get; set; }
    public required string Username { get; set; }
    public required string FirstName { get; set; }
    public string? MiddleName { get; set; }
    public required string LastName { get; set; }
    public string? SecondLastName { get; set; }
    public string? Email { get; set; }
    public required List<string> GroupReferences { get; set; }
    public required List<string> BranchReferences { get; set; }
    public required bool IsActive { get; set; }
    public required DateTime CreatedAtUtc { get; set; }
    public required string CreatedBy { get; set; }
}

public class CreateUserRequest
{
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string FirstName { get; set; }
    public string? MiddleName { get; set; }
    public required string LastName { get; set; }
    public string? SecondLastName { get; set; }
    public string? Email { get; set; }
    public required List<string> GroupReferences { get; set; }
    public List<string> BranchReferences { get; set; } = [];
    public bool IsActive { get; set; } = true;
}

public class UpdateUserRequest
{
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string? SecondLastName { get; set; }
    public string? Email { get; set; }
    public List<string>? GroupReferences { get; set; }
    public List<string>? BranchReferences { get; set; }
    public bool? IsActive { get; set; }
}

public class ResetPasswordRequest
{
    public required string NewPassword { get; set; }
}
