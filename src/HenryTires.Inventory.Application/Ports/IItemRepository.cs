using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Domain.Enums;

namespace HenryTires.Inventory.Application.Ports;

/// <summary>
/// Item repository interface. Implementation inherits from CrudRepository base class
/// which provides basic CRUD operations implementation. Defines custom methods for Item queries and soft-delete.
/// </summary>
public interface IItemRepository
{
    // Basic CRUD operations (implemented by CrudRepository, but overridden for soft-delete filtering)
    Task<Item?> GetByIdAsync(string id);
    Task<IEnumerable<Item>> GetAllAsync(Classification? classification = null);

    // Custom query methods
    Task<Item?> GetByItemCodeAsync(string itemCode);
    Task<IEnumerable<Item>> SearchAsync(string? search, Classification? classification, int page, int pageSize);
    Task<int> CountAsync(string? search, Classification? classification);

    // Custom command methods
    Task<Item> CreateAsync(Item item);
    Task UpdateAsync(Item item);
    Task DeleteAsync(string id, string deletedBy, DateTime deletedAtUtc); // Soft delete
}
