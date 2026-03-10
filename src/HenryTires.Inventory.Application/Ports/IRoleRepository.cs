using HenryTires.Inventory.Domain.Entities;

namespace HenryTires.Inventory.Application.Ports;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(string id);
    Task<IEnumerable<Role>> GetAllAsync();
    Task<IEnumerable<Role>> SearchAsync(string? searchTerm, int page, int pageSize);
    Task<int> CountAsync(string? searchTerm);
    Task<Role?> GetByCodeAsync(string code);
    Task<Role> CreateAsync(Role role);
    Task UpdateAsync(Role role);
    Task DeleteAsync(string id);
}
