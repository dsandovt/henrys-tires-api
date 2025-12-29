using HenryTires.Inventory.Application.Common;
using HenryTires.Inventory.Application.DTOs;
using HenryTires.Inventory.Application.Ports;
using HenryTires.Inventory.Application.Ports.Inbound;
using HenryTires.Inventory.Application.Ports.Outbound;
using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Domain.Enums;

namespace HenryTires.Inventory.Application.UseCases.Inventory;

public class ItemManagementService : IItemManagementService
{
    private readonly IItemRepository _itemRepository;
    private readonly IConsumableItemPriceRepository _priceRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly IIdentityGenerator _identityGenerator;

    public ItemManagementService(
        IItemRepository itemRepository,
        IConsumableItemPriceRepository priceRepository,
        ICurrentUser currentUser,
        IClock clock,
        IIdentityGenerator identityGenerator
    )
    {
        _itemRepository = itemRepository;
        _priceRepository = priceRepository;
        _currentUser = currentUser;
        _clock = clock;
        _identityGenerator = identityGenerator;
    }

    public async Task<ItemDto> CreateItemAsync(CreateItemRequest request)
    {
        if (!Enum.TryParse<Classification>(request.Classification, true, out var classification))
        {
            throw new ValidationException(
                $"Invalid classification: {request.Classification}. Must be 'Good' or 'Service'"
            );
        }

        var existing = await _itemRepository.GetByItemCodeAsync(request.ItemCode);
        if (existing != null && !existing.IsDeleted)
        {
            throw new ValidationException($"Item with code '{request.ItemCode}' already exists");
        }

        if (existing != null && existing.IsDeleted)
        {
            existing.Description = request.Description;
            existing.Classification = classification;
            existing.Notes = request.Notes;
            existing.IsDeleted = false;
            existing.DeletedAtUtc = null;
            existing.DeletedBy = null;
            existing.ModifiedAtUtc = _clock.UtcNow;
            existing.ModifiedBy = _currentUser.Username;

            await _itemRepository.UpdateAsync(existing);
            return ItemDto.FromEntity(existing);
        }

        var item = new Item
        {
            Id = _identityGenerator.GenerateId(),
            ItemCode = request.ItemCode,
            Description = request.Description,
            Classification = classification,
            Notes = request.Notes,
            IsActive = true,
            IsDeleted = false,
            CreatedAtUtc = _clock.UtcNow,
            CreatedBy = _currentUser.Username,
            ModifiedAtUtc = _clock.UtcNow,
            ModifiedBy = _currentUser.Username,
        };

        await _itemRepository.CreateAsync(item);

        if (request.InitialPrice.HasValue && request.InitialPrice.Value > 0)
        {
            var priceRecord = new ConsumableItemPrice
            {
                Id = _identityGenerator.GenerateId(),
                ItemCode = request.ItemCode,
                Currency = request.Currency ?? Currency.USD,
                LatestPrice = request.InitialPrice.Value,
                LatestPriceDateUtc = _clock.UtcNow,
                UpdatedBy = _currentUser.Username,
                History = new List<PriceHistoryEntry>(),
            };

            await _priceRepository.CreateAsync(priceRecord);
        }

        return ItemDto.FromEntity(item);
    }

    public async Task<ItemDto> UpdateItemAsync(string itemCode, UpdateItemRequest request)
    {
        var item = await _itemRepository.GetByItemCodeAsync(itemCode);
        if (item == null)
        {
            throw new NotFoundException($"Item with code '{itemCode}' not found");
        }

        if (item.IsDeleted)
        {
            throw new ValidationException($"Item '{itemCode}' is deleted");
        }

        item.Description = request.Description;
        item.ModifiedAtUtc = _clock.UtcNow;
        item.ModifiedBy = _currentUser.Username;

        await _itemRepository.UpdateAsync(item);

        return ItemDto.FromEntity(item);
    }

    public async Task DeleteItemAsync(string itemCode)
    {
        var item = await _itemRepository.GetByItemCodeAsync(itemCode);
        if (item == null)
        {
            throw new NotFoundException($"Item with code '{itemCode}' not found");
        }

        if (item.IsDeleted)
        {
            throw new ValidationException($"Item '{itemCode}' is already deleted");
        }

        item.IsDeleted = true;
        item.DeletedAtUtc = _clock.UtcNow;
        item.DeletedBy = _currentUser.Username;
        item.ModifiedAtUtc = _clock.UtcNow;
        item.ModifiedBy = _currentUser.Username;

        await _itemRepository.UpdateAsync(item);
    }

    public async Task<ItemDto> GetItemByCodeAsync(string itemCode)
    {
        var item = await _itemRepository.GetByItemCodeAsync(itemCode);
        if (item == null)
        {
            throw new NotFoundException($"Item with code '{itemCode}' not found");
        }

        return ItemDto.FromEntity(item);
    }

    public async Task<ItemDto> GetItemByIdAsync(string itemId)
    {
        var item = await _itemRepository.GetByIdAsync(itemId);
        if (item == null)
        {
            throw new NotFoundException($"Item with ID '{itemId}' not found");
        }

        return ItemDto.FromEntity(item);
    }

    public async Task<ItemListResponse> SearchItemsAsync(
        string? search,
        string? classificationFilter,
        int page,
        int pageSize
    )
    {
        Classification? classification = null;
        if (!string.IsNullOrEmpty(classificationFilter))
        {
            if (
                Enum.TryParse<Classification>(
                    classificationFilter,
                    true,
                    out var parsedClassification
                )
            )
            {
                classification = parsedClassification;
            }
        }

        var items = await _itemRepository.SearchAsync(search, classification, page, pageSize);
        var count = await _itemRepository.CountAsync(search, classification);

        return new ItemListResponse
        {
            Items = items.Select(ItemDto.FromEntity),
            TotalCount = count,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<IEnumerable<ItemDto>> GetAllItemsAsync()
    {
        IEnumerable<Item> items = await _itemRepository.GetAllAsync();
        return items.Select(ItemDto.FromEntity);
    }
}
