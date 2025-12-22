using HenryTires.Inventory.Domain.Entities;

namespace HenryTires.Inventory.Application.Ports;

/// <summary>
/// Branch repository interface. Implementation inherits from CrudRepository base class
/// which provides the basic CRUD operations implementation.
/// </summary>
public interface IBranchRepository
{
    // Basic CRUD operations (implemented by CrudRepository)
    Task<Branch?> GetByIdAsync(string id);
    Task<IEnumerable<Branch>> GetAllAsync();

    // Custom query methods
    Task<Branch?> GetByCodeAsync(string code);
}
