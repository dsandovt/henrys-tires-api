using HenryTires.Inventory.Application.Common;
using HenryTires.Inventory.Application.DTOs;
using HenryTires.Inventory.Application.Ports;
using HenryTires.Inventory.Application.Ports.Inbound;
using HenryTires.Inventory.Application.Ports.Outbound;
using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Domain.Enums;
using HenryTires.Inventory.Domain.Services;

namespace HenryTires.Inventory.Application.UseCases.Inventory;

public class NewTransactionService : INewTransactionService
{
    private readonly IItemRepository _itemRepository;
    private readonly IConsumableItemPriceRepository _priceRepository;
    private readonly IInventorySummaryRepository _summaryRepository;
    private readonly IInventoryTransactionRepository _transactionRepository;
    private readonly StockAvailabilityService _stockAvailabilityService;
    private readonly PriceResolutionService _priceResolutionService;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityGenerator _identityGenerator;

    public NewTransactionService(
        IItemRepository itemRepository,
        IConsumableItemPriceRepository priceRepository,
        IInventorySummaryRepository summaryRepository,
        IInventoryTransactionRepository transactionRepository,
        StockAvailabilityService stockAvailabilityService,
        PriceResolutionService priceResolutionService,
        ICurrentUser currentUser,
        IClock clock,
        IUnitOfWork unitOfWork,
        IIdentityGenerator identityGenerator
    )
    {
        _itemRepository = itemRepository;
        _priceRepository = priceRepository;
        _summaryRepository = summaryRepository;
        _transactionRepository = transactionRepository;
        _stockAvailabilityService = stockAvailabilityService;
        _priceResolutionService = priceResolutionService;
        _currentUser = currentUser;
        _clock = clock;
        _unitOfWork = unitOfWork;
        _identityGenerator = identityGenerator;
    }

    public async Task<NewTransactionDto> CreateInTransactionAsync(
        CreateInTransactionRequest request
    )
    {
        var branchCode = ValidateBranchAccess(request.BranchCode);

        var lines = new List<InventoryTransactionLine>();
        foreach (var lineRequest in request.Lines)
        {
            var item = await _itemRepository.GetByItemCodeAsync(lineRequest.ItemCode);
            if (item == null)
            {
                throw new NotFoundException($"Item with code '{lineRequest.ItemCode}' not found");
            }

            if (item.IsDeleted)
            {
                throw new ValidationException($"Item '{lineRequest.ItemCode}' is deleted");
            }

            if (!Enum.TryParse<ItemCondition>(lineRequest.ItemCondition, true, out var condition))
            {
                throw new ValidationException($"Invalid condition: {lineRequest.ItemCondition}");
            }

            if (lineRequest.Quantity <= 0)
            {
                throw new ValidationException("Quantity must be greater than zero");
            }

            var (unitPrice, currency, priceSource) = await ResolveInPriceAsync(
                lineRequest.ItemCode,
                lineRequest.UnitPrice,
                lineRequest.Currency
            );

            var lineTotal = InventoryTransactionLine.CalculateLineTotal(
                lineRequest.Quantity,
                unitPrice
            );

            var line = new InventoryTransactionLine
            {
                LineId = _identityGenerator.GenerateId(),
                ItemId = item.Id,
                ItemCode = lineRequest.ItemCode,
                Condition = condition,
                Quantity = lineRequest.Quantity,
                UnitPrice = unitPrice,
                Currency = currency,
                PriceSource = priceSource,
                PriceSetByRole = _currentUser.Role.ToString(),
                PriceSetByUser = _currentUser.Username,
                LineTotal = lineTotal,
                CostOfGoodsSold = null, // Not applicable for IN
                PriceNotes = lineRequest.PriceNotes,
                ExecutedAtUtc = _clock.UtcNow,
            };

            lines.Add(line);
        }

        var transactionNumber =
            $"IN-{_clock.UtcNow:yyyyMMdd}-{_identityGenerator.GenerateId().Substring(0, 8).ToUpper()}";

        var transaction = new InventoryTransaction
        {
            Id = _identityGenerator.GenerateId(),
            TransactionNumber = transactionNumber,
            BranchCode = branchCode,
            Type = TransactionType.In,
            Status = TransactionStatus.Draft,
            TransactionDateUtc = request.TransactionDateUtc,
            Notes = request.Notes,
            Lines = lines,
            CreatedAtUtc = _clock.UtcNow,
            CreatedBy = _currentUser.Username,
            ModifiedAtUtc = _clock.UtcNow,
            ModifiedBy = _currentUser.Username,
        };

        await _transactionRepository.CreateAsync(transaction);

        return NewTransactionDto.FromEntity(transaction);
    }

    public async Task<NewTransactionDto> CreateOutTransactionAsync(
        CreateOutTransactionRequest request
    )
    {
        var branchCode = ValidateBranchAccess(request.BranchCode);

        foreach (var lineRequest in request.Lines)
        {
            if (!Enum.TryParse<ItemCondition>(lineRequest.ItemCondition, true, out var condition))
            {
                throw new ValidationException($"Invalid condition: {lineRequest.ItemCondition}");
            }

            if (lineRequest.Quantity <= 0)
            {
                throw new ValidationException("Quantity must be greater than zero");
            }

            var summary = await _summaryRepository.GetByKeyAsync(branchCode, lineRequest.ItemCode);

            var stockCheck = _stockAvailabilityService.CheckAvailability(
                summary,
                condition,
                lineRequest.Quantity
            );

            if (!stockCheck.IsSufficient)
            {
                throw new BusinessException(
                    $"Insufficient stock for {lineRequest.ItemCode} ({lineRequest.ItemCondition}). "
                        + $"Available: {stockCheck.Available}, Requested: {stockCheck.Requested}"
                );
            }
        }

        var lines = new List<InventoryTransactionLine>();
        foreach (var lineRequest in request.Lines)
        {
            var item = await _itemRepository.GetByItemCodeAsync(lineRequest.ItemCode);
            if (item == null)
            {
                throw new NotFoundException($"Item with code '{lineRequest.ItemCode}' not found");
            }

            if (item.IsDeleted)
            {
                throw new ValidationException($"Item '{lineRequest.ItemCode}' is deleted");
            }

            var condition = Enum.Parse<ItemCondition>(lineRequest.ItemCondition, true);

            // CRITICAL: Role-based price resolution for OUT (Sellers cannot override)
            var (unitPrice, currency, priceSource, priceSetByRole) = await ResolveOutPriceAsync(
                lineRequest.ItemCode,
                lineRequest.UnitPrice,
                lineRequest.Currency
            );

            var lineTotal = InventoryTransactionLine.CalculateLineTotal(
                lineRequest.Quantity,
                unitPrice
            );

            var line = new InventoryTransactionLine
            {
                LineId = _identityGenerator.GenerateId(),
                ItemId = item.Id,
                ItemCode = lineRequest.ItemCode,
                Condition = condition,
                Quantity = lineRequest.Quantity,
                UnitPrice = unitPrice,
                Currency = currency,
                PriceSource = priceSource,
                PriceSetByRole = priceSetByRole,
                PriceSetByUser = _currentUser.Username,
                LineTotal = lineTotal,
                CostOfGoodsSold = null, // TODO: Calculate from inventory valuation
                PriceNotes = lineRequest.PriceNotes,
                ExecutedAtUtc = _clock.UtcNow,
            };

            lines.Add(line);
        }

        var transactionNumber =
            $"OUT-{_clock.UtcNow:yyyyMMdd}-{_identityGenerator.GenerateId().Substring(0, 8).ToUpper()}";

        var transaction = new InventoryTransaction
        {
            Id = _identityGenerator.GenerateId(),
            TransactionNumber = transactionNumber,
            BranchCode = branchCode,
            Type = TransactionType.Out,
            Status = TransactionStatus.Draft,
            TransactionDateUtc = request.TransactionDateUtc,
            Notes = request.Notes,
            Lines = lines,
            CreatedAtUtc = _clock.UtcNow,
            CreatedBy = _currentUser.Username,
            ModifiedAtUtc = _clock.UtcNow,
            ModifiedBy = _currentUser.Username,
        };

        await _transactionRepository.CreateAsync(transaction);

        return NewTransactionDto.FromEntity(transaction);
    }

    public async Task<NewTransactionDto> CreateAdjustTransactionAsync(
        CreateAdjustTransactionRequest request
    )
    {
        var branchCode = ValidateBranchAccess(request.BranchCode);

        var lines = new List<InventoryTransactionLine>();
        foreach (var lineRequest in request.Lines)
        {
            var item = await _itemRepository.GetByItemCodeAsync(lineRequest.ItemCode);
            if (item == null)
            {
                throw new NotFoundException($"Item with code '{lineRequest.ItemCode}' not found");
            }

            if (item.IsDeleted)
            {
                throw new ValidationException($"Item '{lineRequest.ItemCode}' is deleted");
            }

            if (!Enum.TryParse<ItemCondition>(lineRequest.ItemCondition, true, out var condition))
            {
                throw new ValidationException($"Invalid condition: {lineRequest.ItemCondition}");
            }

            if (lineRequest.NewQuantity < 0)
            {
                throw new ValidationException("Quantity cannot be negative");
            }

            var (unitPrice, currency, priceSource) = await ResolveAdjustPriceAsync(
                lineRequest.ItemCode,
                lineRequest.UnitPrice,
                lineRequest.Currency
            );

            var lineTotal = InventoryTransactionLine.CalculateLineTotal(
                lineRequest.NewQuantity,
                unitPrice
            );

            var line = new InventoryTransactionLine
            {
                LineId = _identityGenerator.GenerateId(),
                ItemId = item.Id,
                ItemCode = lineRequest.ItemCode,
                Condition = condition,
                Quantity = lineRequest.NewQuantity,
                UnitPrice = unitPrice,
                Currency = currency,
                PriceSource = priceSource,
                PriceSetByRole = _currentUser.Role.ToString(),
                PriceSetByUser = _currentUser.Username,
                LineTotal = lineTotal,
                CostOfGoodsSold = null,
                PriceNotes = lineRequest.PriceNotes,
                ExecutedAtUtc = _clock.UtcNow,
            };

            lines.Add(line);
        }

        var transactionNumber =
            $"ADJ-{_clock.UtcNow:yyyyMMdd}-{_identityGenerator.GenerateId().Substring(0, 8).ToUpper()}";

        var transaction = new InventoryTransaction
        {
            Id = _identityGenerator.GenerateId(),
            TransactionNumber = transactionNumber,
            BranchCode = branchCode,
            Type = TransactionType.Adjust,
            Status = TransactionStatus.Draft,
            TransactionDateUtc = request.TransactionDateUtc,
            Notes = request.Notes,
            Lines = lines,
            CreatedAtUtc = _clock.UtcNow,
            CreatedBy = _currentUser.Username,
            ModifiedAtUtc = _clock.UtcNow,
            ModifiedBy = _currentUser.Username,
        };

        await _transactionRepository.CreateAsync(transaction);

        return NewTransactionDto.FromEntity(transaction);
    }

    public async Task<NewTransactionDto> CommitTransactionAsync(CommitTransactionRequest request)
    {
        using var scope = await _unitOfWork.BeginTransactionAsync();
        try
        {
            var transaction = await _transactionRepository.GetByIdAsync(request.TransactionId);
            if (transaction == null)
            {
                throw new NotFoundException($"Transaction '{request.TransactionId}' not found");
            }

            ValidateBranchAccess(transaction.BranchCode);

            transaction.Commit(_currentUser.Username, _clock.UtcNow);

            var itemGroups = transaction.Lines.GroupBy(l => l.ItemCode);
            foreach (var itemGroup in itemGroups)
            {
                var itemCode = itemGroup.Key;

                var summary = await _summaryRepository.GetByKeyAsync(
                    transaction.BranchCode,
                    itemCode,
                    scope
                );

                if (summary == null)
                {
                    summary = new InventorySummary
                    {
                        Id = _identityGenerator.GenerateId(),
                        BranchCode = transaction.BranchCode,
                        ItemCode = itemCode,
                        Entries = new List<InventoryEntry>(),
                        OnHandTotal = 0,
                        ReservedTotal = 0,
                        Version = 0,
                        UpdatedAtUtc = _clock.UtcNow,
                    };
                }

                summary.ApplyTransaction(transaction);
                summary.UpdatedAtUtc = _clock.UtcNow;

                await _summaryRepository.UpsertWithVersionCheckAsync(summary, scope);
            }

            await _transactionRepository.UpdateAsync(transaction);

            await scope.CommitAsync();

            return NewTransactionDto.FromEntity(transaction);
        }
        catch (ConcurrencyException)
        {
            await scope.RollbackAsync();
            throw; // Re-throw concurrency exception
        }
        catch
        {
            await scope.RollbackAsync();
            throw;
        }
    }

    public async Task<NewTransactionDto> CancelTransactionAsync(CancelTransactionRequest request)
    {
        var transaction = await _transactionRepository.GetByIdAsync(request.TransactionId);
        if (transaction == null)
        {
            throw new NotFoundException($"Transaction '{request.TransactionId}' not found");
        }

        ValidateBranchAccess(transaction.BranchCode);

        transaction.Cancel();
        transaction.ModifiedAtUtc = _clock.UtcNow;
        transaction.ModifiedBy = _currentUser.Username;

        await _transactionRepository.UpdateAsync(transaction);

        return NewTransactionDto.FromEntity(transaction);
    }

    public async Task<NewTransactionDto> GetTransactionByIdAsync(string transactionId)
    {
        var transaction = await _transactionRepository.GetByIdAsync(transactionId);
        if (transaction == null)
        {
            throw new NotFoundException($"Transaction '{transactionId}' not found");
        }

        ValidateBranchAccess(transaction.BranchCode);

        return NewTransactionDto.FromEntity(transaction);
    }

    public async Task<NewTransactionListResponse> GetTransactionsByBranchAsync(
        string? branchCode,
        TransactionType? type,
        TransactionStatus? status,
        int page,
        int pageSize
    )
    {
        var validatedBranchCode = ValidateBranchAccessForQuery(branchCode);

        var transactions = await _transactionRepository.SearchAsync(
            validatedBranchCode,
            null, // from
            null, // to
            type,
            status,
            null, // itemCode
            null, // condition
            page,
            pageSize
        );

        var count = await _transactionRepository.CountAsync(
            validatedBranchCode,
            null, // from
            null, // to
            type,
            status,
            null, // itemCode
            null
        ); // condition

        return new NewTransactionListResponse
        {
            Items = transactions.Select(NewTransactionDto.FromEntity),
            TotalCount = count,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<InventorySummaryDto?> GetInventorySummaryAsync(
        string? branchCode,
        string itemCode
    )
    {
        var validatedBranchCode = ValidateBranchAccess(branchCode);

        var summary = await _summaryRepository.GetByKeyAsync(validatedBranchCode, itemCode);

        return summary == null ? null : InventorySummaryDto.FromEntity(summary);
    }

    public async Task<InventorySummaryListResponse> GetInventorySummariesByBranchAsync(
        string? branchCode,
        string? search,
        ItemCondition? condition,
        int page,
        int pageSize
    )
    {
        var validatedBranchCode = ValidateBranchAccessForQuery(branchCode);

        var summaries = await _summaryRepository.GetByBranchAsync(
            validatedBranchCode,
            search,
            condition,
            page,
            pageSize
        );

        var count = await _summaryRepository.CountByBranchAsync(
            validatedBranchCode,
            search,
            condition
        );

        StockTotalsDto? generalStock = null;

        if (!condition.HasValue) // Only calculate general totals when no specific condition filter is applied
        {
            var allSummariesForTotals = await _summaryRepository.GetByBranchAsync(
                validatedBranchCode,
                search,
                null, // No condition filter
                1,
                int.MaxValue
            ); // Get all items to calculate totals

            int newStock = 0;
            int usedStock = 0;

            foreach (var summary in allSummariesForTotals)
            {
                foreach (var entry in summary.Entries)
                {
                    if (entry.Condition == ItemCondition.New)
                    {
                        newStock += entry.OnHand;
                    }
                    else if (entry.Condition == ItemCondition.Used)
                    {
                        usedStock += entry.OnHand;
                    }
                }
            }

            generalStock = new StockTotalsDto
            {
                NewStock = newStock,
                UsedStock = usedStock,
                TotalStock = newStock + usedStock,
            };
        }

        return new InventorySummaryListResponse
        {
            Items = summaries.Select(InventorySummaryDto.FromEntity),
            TotalCount = count,
            Page = page,
            PageSize = pageSize,
            GeneralStock = generalStock,
        };
    }

    private string? ValidateBranchAccessForQuery(string? branchCode)
    {
        if (_currentUser.Role == Role.Admin)
        {
            return branchCode;
        }
        else
        {
            if (_currentUser.BranchId == null)
            {
                throw new UnauthorizedException("User does not have an assigned branch");
            }

            if (_currentUser.BranchCode == null)
            {
                throw new UnauthorizedException(
                    $"User's assigned branch (ID: {_currentUser.BranchId}) not found in system. Please contact administrator to fix branch data."
                );
            }

            return _currentUser.BranchCode;
        }
    }

    private string ValidateBranchAccess(string? branchCode)
    {
        if (_currentUser.Role == Role.Admin)
        {
            if (string.IsNullOrWhiteSpace(branchCode))
            {
                throw new ValidationException("BranchCode is required for Admin users");
            }
            return branchCode;
        }
        else
        {
            if (_currentUser.BranchId == null)
            {
                throw new UnauthorizedException("User does not have an assigned branch");
            }

            if (_currentUser.BranchCode == null)
            {
                throw new UnauthorizedException(
                    $"User's assigned branch (ID: {_currentUser.BranchId}) not found in system. Please contact administrator to fix branch data."
                );
            }

            return _currentUser.BranchCode;
        }
    }

    private async Task<(
        decimal unitPrice,
        string currency,
        PriceSource priceSource
    )> ResolveInPriceAsync(string itemCode, decimal? manualPrice, string? manualCurrency)
    {
        if (manualPrice.HasValue)
        {
            if (manualPrice.Value < 0)
            {
                throw new ValidationException("Unit price cannot be negative");
            }
            var currency = manualCurrency ?? "USD";
            return (manualPrice.Value, currency, PriceSource.Manual);
        }

        var itemPrice = await _priceRepository.GetByItemCodeAsync(itemCode);
        if (itemPrice != null)
        {
            return (itemPrice.LatestPrice, itemPrice.Currency, PriceSource.ConsumableItemPrice);
        }

        return (0m, "USD", PriceSource.SystemDefault);
    }

    private async Task<(
        decimal unitPrice,
        string currency,
        PriceSource priceSource,
        string priceSetByRole
    )> ResolveOutPriceAsync(string itemCode, decimal? manualPrice, string? manualCurrency)
    {
        if (manualPrice.HasValue)
        {
            if (_currentUser.Role != Role.Admin && _currentUser.Role != Role.Supervisor)
            {
                throw new UnauthorizedException(
                    $"User '{_currentUser.Username}' with role '{_currentUser.Role}' cannot override selling prices. "
                        + "Only Admin or Supervisor roles are authorized to set custom prices on OUT transactions."
                );
            }

            if (manualPrice.Value < 0)
            {
                throw new ValidationException("Unit price cannot be negative");
            }

            return (
                manualPrice.Value,
                manualCurrency ?? "USD",
                PriceSource.Manual,
                _currentUser.Role.ToString()
            );
        }

        var itemPrice = await _priceRepository.GetByItemCodeAsync(itemCode);
        if (itemPrice != null)
        {
            return (
                itemPrice.LatestPrice,
                itemPrice.Currency,
                PriceSource.ConsumableItemPrice,
                "System"
            );
        }

        throw new BusinessException(
            $"Cannot create OUT transaction for item '{itemCode}': No selling price found in ConsumableItemPrice. "
                + "Please set the price first (Admin only) or contact an administrator."
        );
    }

    private async Task<(
        decimal unitPrice,
        string currency,
        PriceSource priceSource
    )> ResolveAdjustPriceAsync(string itemCode, decimal? manualPrice, string? manualCurrency)
    {
        if (manualPrice.HasValue)
        {
            if (manualPrice.Value < 0)
            {
                throw new ValidationException("Unit price cannot be negative");
            }
            return (manualPrice.Value, manualCurrency ?? "USD", PriceSource.Manual);
        }

        var itemPrice = await _priceRepository.GetByItemCodeAsync(itemCode);
        if (itemPrice != null)
        {
            return (itemPrice.LatestPrice, itemPrice.Currency, PriceSource.ConsumableItemPrice);
        }

        return (0m, "USD", PriceSource.SystemDefault);
    }
}
