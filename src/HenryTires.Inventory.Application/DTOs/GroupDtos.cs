namespace HenryTires.Inventory.Application.DTOs;

public class GroupDto
{
    public required string Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required List<string> RoleReferences { get; set; }
    public required bool IsActive { get; set; }
}

public class CreateGroupDto
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required List<string> RoleReferences { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateGroupDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public List<string>? RoleReferences { get; set; }
    public bool? IsActive { get; set; }
}

public class AssignRolesRequest
{
    public required List<string> RoleReferences { get; set; }
}

public class RemoveRolesRequest
{
    public required List<string> RoleReferences { get; set; }
}