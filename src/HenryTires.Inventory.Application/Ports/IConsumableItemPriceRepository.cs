using HenryTires.Inventory.Domain.Entities;

namespace HenryTires.Inventory.Application.Ports;

public interface IConsumableItemPriceRepository
{
    Task<IEnumerable<ConsumableItemPrice>> GetAllAsync();

    Task<ConsumableItemPrice?> GetByItemCodeAsync(string itemCode);
    Task<ConsumableItemPrice> CreateAsync(ConsumableItemPrice price);
    Task UpdateAsync(ConsumableItemPrice price);
}
