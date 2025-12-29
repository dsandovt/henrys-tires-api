using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Domain.Enums;

namespace HenryTires.Inventory.Application.Ports;

public interface IItemRepository
{
    Task<Item?> GetByIdAsync(string id);
    Task<IEnumerable<Item>> GetAllAsync(Classification? classification = null);

    Task<Item?> GetByItemCodeAsync(string itemCode);
    Task<IEnumerable<Item>> SearchAsync(
        string? search,
        Classification? classification,
        int page,
        int pageSize
    );
    Task<int> CountAsync(string? search, Classification? classification);

    // Custom command methods
    Task<Item> CreateAsync(Item item);
    Task UpdateAsync(Item item);
    Task DeleteAsync(string id, string deletedBy, DateTime deletedAtUtc); // Soft delete
}
