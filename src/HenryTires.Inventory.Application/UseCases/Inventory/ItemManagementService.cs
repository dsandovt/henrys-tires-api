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
    private readonly IInventorySummaryRepository _summaryRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly IIdentityGenerator _identityGenerator;
    private readonly IUnitOfWork _unitOfWork;

    public ItemManagementService(
        IItemRepository itemRepository,
        IConsumableItemPriceRepository priceRepository,
        IInventorySummaryRepository summaryRepository,
        ICurrentUser currentUser,
        IClock clock,
        IIdentityGenerator identityGenerator,
        IUnitOfWork unitOfWork
    )
    {
        _itemRepository = itemRepository;
        _priceRepository = priceRepository;
        _summaryRepository = summaryRepository;
        _currentUser = currentUser;
        _clock = clock;
        _identityGenerator = identityGenerator;
        _unitOfWork = unitOfWork;
    }

    public async Task<ItemDto> CreateItemAsync(CreateItemRequest request)
    {
        // Validate classification
        if (!Enum.TryParse<Classification>(request.Classification, true, out var classification))
        {
            throw new ValidationException(
                $"Invalid classification: {request.Classification}. Must be 'Good' or 'Service'"
            );
        }

        // Check uniqueness - ItemCode must be unique
        var existing = await _itemRepository.GetByItemCodeAsync(request.ItemCode);
        if (existing != null && !existing.IsDeleted)
        {
            throw new ConflictException($"Item with code '{request.ItemCode}' already exists");
        }

        // Handle soft-deleted item restoration
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

        // Determine branch code for InventorySummary creation
        var branchCode = ResolveBranchCode();

        // Use transaction to ensure atomicity
        using var scope = await _unitOfWork.BeginTransactionAsync();
        try
        {
            // Create the Item
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

            // Auto-create InventorySummary for Goods
            if (classification == Classification.Good)
            {
                await CreateInventorySummaryIfNotExistsAsync(
                    branchCode,
                    request.ItemCode,
                    scope
                );
            }

            // Auto-create ConsumableItemPrice for all items (Goods and Services)
            await CreateConsumableItemPriceIfNotExistsAsync(
                request.ItemCode,
                request.InitialPrice ?? 0m,
                request.Currency ?? Currency.USD,
                scope
            );

            await scope.CommitAsync();
            return ItemDto.FromEntity(item);
        }
        catch
        {
            await scope.RollbackAsync();
            throw;
        }
    }

    private string ResolveBranchCode()
    {
        // For now, use user's assigned branch code
        // Admin users would need to have a default branch or specify one
        if (string.IsNullOrWhiteSpace(_currentUser.BranchCode))
        {
            throw new ValidationException(
                "User does not have an assigned branch. Cannot create Item inventory records."
            );
        }

        return _currentUser.BranchCode;
    }

    private async Task CreateInventorySummaryIfNotExistsAsync(
        string branchCode,
        string itemCode,
        ITransactionScope transactionScope
    )
    {
        // Check if InventorySummary already exists
        var existing = await _summaryRepository.GetByKeyAsync(
            branchCode,
            itemCode,
            transactionScope
        );

        if (existing != null)
        {
            // Already exists - this could happen if created by a transaction first
            return;
        }

        // Create new InventorySummary with empty entries
        var summary = new InventorySummary
        {
            Id = _identityGenerator.GenerateId(),
            BranchCode = branchCode,
            ItemCode = itemCode,
            Entries = new List<InventoryEntry>(),
            OnHandTotal = 0,
            ReservedTotal = 0,
            Version = 1,
            UpdatedAtUtc = _clock.UtcNow,
        };

        await _summaryRepository.UpsertAsync(summary, transactionScope);
    }

    private async Task CreateConsumableItemPriceIfNotExistsAsync(
        string itemCode,
        decimal initialPrice,
        Currency currency,
        ITransactionScope transactionScope
    )
    {
        // Check if price already exists (unique constraint enforced at DB level)
        var existing = await _priceRepository.GetByItemCodeAsync(itemCode);

        if (existing != null)
        {
            // Already exists - could happen if this method is called multiple times
            return;
        }

        // Create new ConsumableItemPrice
        var priceRecord = new ConsumableItemPrice
        {
            Id = _identityGenerator.GenerateId(),
            ItemCode = itemCode,
            Currency = currency,
            LatestPrice = initialPrice,
            LatestPriceDateUtc = _clock.UtcNow,
            UpdatedBy = _currentUser.Username,
            History = new List<PriceHistoryEntry>(),
        };

        await _priceRepository.CreateAsync(priceRecord);
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
