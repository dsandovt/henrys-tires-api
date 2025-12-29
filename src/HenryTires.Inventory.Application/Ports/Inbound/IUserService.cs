using HenryTires.Inventory.Application.DTOs;

namespace HenryTires.Inventory.Application.Ports.Inbound;

public interface IUserService
{
    Task<UserListResponse> GetUsersAsync(int page, int pageSize, string? search);
    Task<UserDto> GetUserByIdAsync(string id);
    Task<UserDto> CreateUserAsync(CreateUserRequest request);
    Task<UserDto> UpdateUserAsync(string id, UpdateUserRequest request);
    Task DeleteUserAsync(string id);
    Task<UserDto> ToggleUserStatusAsync(string id);
}
