namespace HenryTires.Inventory.Application.DTOs;

public class LoginRequest
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}

public class LoginResponse
{
    public required string Token { get; set; }
    public required string Username { get; set; }
    public required List<string> GroupReferences { get; set; }
    public required List<string> RoleCodes { get; set; }
    public required List<string> BranchReferences { get; set; }
}
