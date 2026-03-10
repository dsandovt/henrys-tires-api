using HenryTires.Inventory.Domain.Entities;

namespace HenryTires.Inventory.Application.Ports;

public interface IGroupRepository
{
    Task<Group?> GetByIdAsync(string id);
    Task<IEnumerable<Group>> GetAllAsync();
    Task<IEnumerable<Group>> SearchAsync(string? searchTerm, int page, int pageSize);
    Task<int> CountAsync(string? searchTerm);
    Task<Group?> GetByCodeAsync(string code);
    Task<Group> CreateAsync(Group group);
    Task UpdateAsync(Group group);
    Task DeleteAsync(string id);
    Task<IEnumerable<Group>> GetByIdsAsync(List<string> ids);
}
