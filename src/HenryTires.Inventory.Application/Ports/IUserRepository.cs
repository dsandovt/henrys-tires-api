using HenryTires.Inventory.Domain.Entities;

namespace HenryTires.Inventory.Application.Ports;

/// <summary>
/// User repository interface. Implementation inherits from CrudRepository base class
/// which provides the basic CRUD operations implementation.
/// </summary>
public interface IUserRepository
{
    // Basic CRUD operations (implemented by CrudRepository)
    Task<User?> GetByIdAsync(string id);
    Task<IEnumerable<User>> GetAllAsync();

    // Custom query and command methods
    Task<User?> GetByUsernameAsync(string username);
    Task<User> CreateAsync(User user);
    Task<IEnumerable<User>> SearchAsync(string? searchTerm, int page, int pageSize);
    Task<int> CountAsync(string? searchTerm);
    Task UpdateAsync(User user);
    Task DeleteAsync(string id);
}
