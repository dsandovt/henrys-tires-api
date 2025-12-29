using HenryTires.Inventory.Application.Common;
using HenryTires.Inventory.Application.DTOs;
using HenryTires.Inventory.Application.Ports;
using HenryTires.Inventory.Application.Ports.Inbound;
using HenryTires.Inventory.Application.Ports.Outbound;
using HenryTires.Inventory.Domain.Entities;

namespace HenryTires.Inventory.Application.UseCases.Inventory;

public class PriceManagementService : IPriceManagementService
{
    private readonly IItemRepository _itemRepository;
    private readonly IConsumableItemPriceRepository _priceRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly IIdentityGenerator _identityGenerator;

    public PriceManagementService(
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

    public async Task<ConsumableItemPriceDto> UpdateItemPriceAsync(
        string itemCode,
        UpdateItemPriceRequest request
    )
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

        if (request.NewPrice <= 0)
        {
            throw new ValidationException("Price must be greater than zero");
        }

        var priceRecord = await _priceRepository.GetByItemCodeAsync(itemCode);

        if (priceRecord == null)
        {
            priceRecord = new ConsumableItemPrice
            {
                Id = _identityGenerator.GenerateId(),
                ItemCode = itemCode,
                Currency = request.Currency,
                LatestPrice = request.NewPrice,
                LatestPriceDateUtc = _clock.UtcNow,
                UpdatedBy = _currentUser.Username,
                History = new List<PriceHistoryEntry>(),
            };

            await _priceRepository.CreateAsync(priceRecord);
        }
        else
        {
            priceRecord.UpdatePrice(request.NewPrice, _currentUser.Username, _clock.UtcNow);

            priceRecord.Currency = request.Currency;

            await _priceRepository.UpdateAsync(priceRecord);
        }

        return ConsumableItemPriceDto.FromEntity(priceRecord);
    }

    public async Task<ConsumableItemPriceDto?> GetItemPriceAsync(string itemCode)
    {
        var priceRecord = await _priceRepository.GetByItemCodeAsync(itemCode);
        return priceRecord == null ? null : ConsumableItemPriceDto.FromEntity(priceRecord);
    }

    public async Task<ConsumableItemPriceWithHistoryDto?> GetItemPriceWithHistoryAsync(
        string itemCode
    )
    {
        var priceRecord = await _priceRepository.GetByItemCodeAsync(itemCode);
        return priceRecord == null
            ? null
            : ConsumableItemPriceWithHistoryDto.FromEntity(priceRecord);
    }

    public async Task<IEnumerable<ConsumableItemPriceDto>> GetAllItemPricesAsync()
    {
        var prices = await _priceRepository.GetAllAsync();
        return prices.Select(ConsumableItemPriceDto.FromEntity);
    }
}
