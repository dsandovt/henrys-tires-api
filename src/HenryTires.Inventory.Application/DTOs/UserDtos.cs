namespace HenryTires.Inventory.Application.DTOs;

public class UserDto
{
    public required string Id { get; set; }
    public required string Username { get; set; }
    public required string Role { get; set; }
    public string? BranchId { get; set; }
    public required bool IsActive { get; set; }
    public required DateTime CreatedAtUtc { get; set; }
    public required string CreatedBy { get; set; }
}

public class CreateUserRequest
{
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string Role { get; set; } // "Admin" or "BranchUser"
    public string? BranchId { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateUserRequest
{
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? Role { get; set; }
    public string? BranchId { get; set; }
    public bool? IsActive { get; set; }
}

public class UserListResponse
{
    public required IEnumerable<UserDto> Items { get; set; }
    public required int TotalCount { get; set; }
    public required int Page { get; set; }
    public required int PageSize { get; set; }
}
