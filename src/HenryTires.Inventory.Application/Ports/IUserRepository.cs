using HenryTires.Inventory.Domain.Entities;

namespace HenryTires.Inventory.Application.Ports;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(string id);
    Task<IEnumerable<User>> GetAllAsync();

    Task<User?> GetByUsernameAsync(string username);
    Task<User> CreateAsync(User user);
    Task<IEnumerable<User>> SearchAsync(string? searchTerm, int page, int pageSize);
    Task<int> CountAsync(string? searchTerm);
    Task UpdateAsync(User user);
    Task DeleteAsync(string id);
}
