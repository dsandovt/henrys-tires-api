using HenryTires.Inventory.Domain.Entities;

namespace HenryTires.Inventory.Application.Ports;

/// <summary>
/// Consumable item price repository interface. Implementation inherits from CrudRepository.
/// </summary>
public interface IConsumableItemPriceRepository
{
    // Basic CRUD operations (implemented by CrudRepository)
    Task<IEnumerable<ConsumableItemPrice>> GetAllAsync();

    // Custom query and command methods
    Task<ConsumableItemPrice?> GetByItemCodeAsync(string itemCode);
    Task<ConsumableItemPrice> CreateAsync(ConsumableItemPrice price);
    Task UpdateAsync(ConsumableItemPrice price);
}
