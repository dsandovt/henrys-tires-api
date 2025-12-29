using HenryTires.Inventory.Application.DTOs;

namespace HenryTires.Inventory.Application.Ports.Inbound;

public interface IPriceManagementService
{
    Task<ConsumableItemPriceDto> UpdateItemPriceAsync(
        string itemCode,
        UpdateItemPriceRequest request
    );
    Task<ConsumableItemPriceDto?> GetItemPriceAsync(string itemCode);
    Task<ConsumableItemPriceWithHistoryDto?> GetItemPriceWithHistoryAsync(string itemCode);
    Task<IEnumerable<ConsumableItemPriceDto>> GetAllItemPricesAsync();
}
