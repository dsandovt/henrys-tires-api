using HenryTires.Inventory.Application.DTOs;

namespace HenryTires.Inventory.Application.Ports.Inbound;

/// <summary>
/// Inbound port for item management operations.
/// </summary>
public interface IItemManagementService
{
    Task<ItemDto> CreateItemAsync(CreateItemRequest request);
    Task<ItemDto> UpdateItemAsync(string itemCode, UpdateItemRequest request);
    Task DeleteItemAsync(string itemCode);
    Task<ItemDto> GetItemByCodeAsync(string itemCode);
    Task<ItemDto> GetItemByIdAsync(string itemId);
    Task<ItemListResponse> SearchItemsAsync(string? search, string? classificationFilter, int page, int pageSize);
    Task<IEnumerable<ItemDto>> GetAllItemsAsync();
}
