using HenryTires.Inventory.Application.Common;
using HenryTires.Inventory.Application.DTOs;

namespace HenryTires.Inventory.Application.Ports.Inbound;

public interface IRoleService
{
    Task<RoleDto> CreateRoleAsync(CreateRoleDto request);
    Task<RoleDto> UpdateRoleAsync(string id, UpdateRoleDto request);
    Task DeleteRoleAsync(string id);
    Task<RoleDto> GetRoleByIdAsync(string id);
    Task<IEnumerable<RoleDto>> GetAllRolesAsync();
    Task<PaginatedResponse<RoleDto>> SearchRolesAsync(string? search, int page, int pageSize);
}
