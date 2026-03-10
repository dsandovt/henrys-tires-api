using HenryTires.Inventory.Application.Common;
using HenryTires.Inventory.Application.DTOs;

namespace HenryTires.Inventory.Application.Ports.Inbound;

public interface IGroupService
{
    Task<GroupDto> CreateGroupAsync(CreateGroupDto request);
    Task<GroupDto> UpdateGroupAsync(string id, UpdateGroupDto request);
    Task DeleteGroupAsync(string id);
    Task<GroupDto> GetGroupByIdAsync(string id);
    Task<IEnumerable<GroupDto>> GetAllGroupsAsync();
    Task<PaginatedResponse<GroupDto>> SearchGroupsAsync(string? search, int page, int pageSize);
    Task<GroupDto> AssignRolesToGroupAsync(string groupId, List<string> roleIds);
    Task<GroupDto> RemoveRolesFromGroupAsync(string groupId, List<string> roleIds);
}
