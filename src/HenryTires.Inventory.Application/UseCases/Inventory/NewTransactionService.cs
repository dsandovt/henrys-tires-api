using HenryTires.Inventory.Application.Common;
using HenryTires.Inventory.Application.DTOs;
using HenryTires.Inventory.Application.Ports;
using HenryTires.Inventory.Application.Ports.Inbound;
using HenryTires.Inventory.Application.Ports.Outbound;
using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Domain.Enums;
using HenryTires.Inventory.Domain.ValueObjects;

namespace HenryTires.Inventory.Application.UseCases.Inventory;

public class NewTransactionService : INewTransactionService
{
    private readonly IItemRepository _itemRepository;
    private readonly IInventorySummaryRepository _summaryRepository;
    private readonly IInventoryTransactionRepository _transactionRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityGenerator _identityGenerator;
    private readonly ISequenceGenerator _sequenceGenerator;

    public NewTransactionService(
        IItemRepository itemRepository,
        IInventorySummaryRepository summaryRepository,
        IInventoryTransactionRepository transactionRepository,
        IBranchRepository branchRepository,
        ICurrentUser currentUser,
        IClock clock,
        IUnitOfWork unitOfWork,
        IIdentityGenerator identityGenerator,
        ISequenceGenerator sequenceGenerator
    )
    {
        _itemRepository = itemRepository;
        _summaryRepository = summaryRepository;
        _transactionRepository = transactionRepository;
        _branchRepository = branchRepository;
        _currentUser = currentUser;
        _clock = clock;
        _unitOfWork = unitOfWork;
        _identityGenerator = identityGenerator;
        _sequenceGenerator = sequenceGenerator;
    }

    public async Task<NewTransactionDto> CreateAdjustTransactionAsync(
        CreateAdjustTransactionRequest request
    )
    {
        var branchCode = ValidateBranchAccess(request.BranchCode);

        var branch = await _branchRepository.GetByCodeAsync(branchCode)
            ?? throw new NotFoundException($"Branch {branchCode} not found");

        var lines = new List<InventoryTransactionLine>();
        foreach (var lineRequest in request.Lines)
        {
            var item = await _itemRepository.GetByItemCodeAsync(lineRequest.ItemCode);
            if (item == null)
                throw new NotFoundException($"Item with code '{lineRequest.ItemCode}' not found");

            if (item.IsDeleted)
                throw new ValidationException($"Item '{lineRequest.ItemCode}' is deleted");

            if (!Enum.TryParse<ItemCondition>(lineRequest.Condition, true, out var condition))
                throw new ValidationException($"Invalid condition: {lineRequest.Condition}");

            if (lineRequest.NewQuantity < 0)
                throw new ValidationException("Quantity cannot be negative");

            lines.Add(new InventoryTransactionLine
            {
                LineId = _identityGenerator.GenerateId(),
                ItemCode = lineRequest.ItemCode,
                Condition = condition,
                Quantity = lineRequest.NewQuantity,
            });
        }

        var sequenceName = $"adj-{branchCode}";
        var sequence = await _sequenceGenerator.GetNextSequenceAsync(sequenceName);
        var adjustmentNumber = $"{branchCode}-ADJ-{sequence:D7}";

        var transactionId = _identityGenerator.GenerateId();

        var userLite = _currentUser.ToUserLite();

        var transaction = new InventoryTransaction
        {
            Id = transactionId,
            BranchReference = branch.Id,
            BranchCode = branchCode,
            Initiator = new EntityKey
            {
                Reference = transactionId,
                ReferenceNumber = adjustmentNumber,
                EntityDefinitionCode = InitiatorType.StockAdjustment,
            },
            Status = InventoryTransactionStatus.Draft,
            TransactionDateUtc = request.TransactionDateUtc ?? _clock.UtcNow,
            Notes = request.Notes,
            Lines = lines,
            StatusHistory = new List<StatusHistoryEntry<InventoryTransactionStatus>>
            {
                new()
                {
                    Date = _clock.UtcNow,
                    Status = InventoryTransactionStatus.Draft,
                    User = userLite,
                },
            },
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
            var transaction = await _transactionRepository.GetByIdAsync(request.TransactionId)
                ?? throw new NotFoundException($"Transaction '{request.TransactionId}' not found");

            ValidateBranchAccess(transaction.BranchCode);

            var userLite = _currentUser.ToUserLite();
            transaction.Commit(userLite, _clock.UtcNow);

            var itemGroups = transaction.Lines.GroupBy(l => l.ItemCode);
            foreach (var itemGroup in itemGroups)
            {
                var itemCode = itemGroup.Key;

                var summary = await _summaryRepository.GetByKeyAsync(
                    transaction.BranchReference,
                    itemCode,
                    scope
                );

                if (summary == null)
                {
                    summary = new InventorySummary
                    {
                        Id = _identityGenerator.GenerateId(),
                        BranchReference = transaction.BranchReference,
                        BranchCode = transaction.BranchCode,
                        ItemCode = itemCode,
                        Entries = new List<InventoryEntry>(),
                        OnHandTotal = 0,
                        ReservedTotal = 0,
                        Version = 0,
                        UpdatedAtUtc = _clock.UtcNow,
                    };
                }

                // Determine stock operation based on initiator type
                foreach (var line in itemGroup)
                {
                    switch (transaction.Initiator.EntityDefinitionCode)
                    {
                        case InitiatorType.PurchaseOrder:
                            summary.IncreaseStock(line.ItemCode, line.Condition, line.Quantity, transaction.TransactionDateUtc);
                            break;
                        case InitiatorType.Sale:
                            summary.DecreaseStock(line.ItemCode, line.Condition, line.Quantity, transaction.TransactionDateUtc);
                            break;
                        case InitiatorType.StockAdjustment:
                            summary.OverrideStock(line.ItemCode, line.Condition, line.Quantity, transaction.TransactionDateUtc);
                            break;
                        case InitiatorType.StockLoss:
                            summary.DecreaseStock(line.ItemCode, line.Condition, line.Quantity, transaction.TransactionDateUtc);
                            break;
                        default:
                            throw new BusinessException(
                                $"Unsupported initiator type: {transaction.Initiator.EntityDefinitionCode}");
                    }
                }

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
            throw;
        }
        catch
        {
            await scope.RollbackAsync();
            throw;
        }
    }

    public async Task<NewTransactionDto> CancelTransactionAsync(CancelTransactionRequest request)
    {
        var transaction = await _transactionRepository.GetByIdAsync(request.TransactionId)
            ?? throw new NotFoundException($"Transaction '{request.TransactionId}' not found");

        ValidateBranchAccess(transaction.BranchCode);

        var userLite = _currentUser.ToUserLite();
        transaction.Cancel(userLite, _clock.UtcNow);

        await _transactionRepository.UpdateAsync(transaction);

        return NewTransactionDto.FromEntity(transaction);
    }

    public async Task<NewTransactionDto> GetTransactionByIdAsync(string transactionId)
    {
        var transaction = await _transactionRepository.GetByIdAsync(transactionId)
            ?? throw new NotFoundException($"Transaction '{transactionId}' not found");

        ValidateBranchAccess(transaction.BranchCode);

        return NewTransactionDto.FromEntity(transaction);
    }

    public async Task<PaginatedResponse<NewTransactionDto>> GetTransactionsByBranchAsync(
        string? branchReference,
        InitiatorType? initiatorType,
        InventoryTransactionStatus? status,
        int page,
        int pageSize
    )
    {
        var validatedBranchReference = ValidateBranchAccessForQuery(branchReference);

        var transactions = await _transactionRepository.SearchAsync(
            validatedBranchReference,
            null, null,
            initiatorType,
            status,
            null, null,
            page,
            pageSize
        );

        var count = await _transactionRepository.CountAsync(
            validatedBranchReference,
            null, null,
            initiatorType,
            status,
            null, null
        );

        return new PaginatedResponse<NewTransactionDto>
        {
            Items = transactions.Select(NewTransactionDto.FromEntity),
            TotalCount = count,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<InventorySummaryDto?> GetInventorySummaryAsync(
        string? branchReference,
        string itemCode
    )
    {
        var validatedBranchReference = ValidateBranchAccessForQuery(branchReference)
            ?? throw new ValidationException("Branch is required");

        var summary = await _summaryRepository.GetByKeyAsync(validatedBranchReference, itemCode);

        return summary == null ? null : InventorySummaryDto.FromEntity(summary);
    }

    public async Task<InventorySummaryListResponse> GetInventorySummariesByBranchAsync(
        string? branchReference,
        string? search,
        ItemCondition? condition,
        int page,
        int pageSize
    )
    {
        var validatedBranchReference = ValidateBranchAccessForQuery(branchReference);

        var summaries = await _summaryRepository.GetByBranchAsync(
            validatedBranchReference, search, condition, page, pageSize
        );

        var count = await _summaryRepository.CountByBranchAsync(
            validatedBranchReference, search, condition
        );

        StockTotalsDto? generalStock = null;

        if (!condition.HasValue)
        {
            var allSummariesForTotals = await _summaryRepository.GetByBranchAsync(
                validatedBranchReference, search, null, 1, int.MaxValue
            );

            int newStock = 0;
            int usedStock = 0;

            foreach (var summary in allSummariesForTotals)
            {
                foreach (var entry in summary.Entries)
                {
                    if (entry.Condition == ItemCondition.New)
                        newStock += entry.OnHand;
                    else if (entry.Condition == ItemCondition.Used)
                        usedStock += entry.OnHand;
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

    private string? ValidateBranchAccessForQuery(string? branchReference)
    {
        if (_currentUser.HasRole("ADMIN"))
            return branchReference;

        if (_currentUser.BranchReferences.Count == 0)
            throw new UnauthorizedException("User does not have an assigned branch");

        if (!string.IsNullOrEmpty(branchReference) && _currentUser.CanAccessBranch(branchReference))
            return branchReference;

        return _currentUser.BranchReferences[0];
    }

    private string ValidateBranchAccess(string? branchCode)
    {
        if (_currentUser.HasRole("ADMIN"))
        {
            if (string.IsNullOrWhiteSpace(branchCode))
                throw new ValidationException("BranchCode is required for Admin users");
            return branchCode;
        }

        if (_currentUser.BranchReferences.Count == 0)
            throw new UnauthorizedException("User does not have an assigned branch");

        if (_currentUser.BranchCodes.Count == 0)
            throw new UnauthorizedException(
                $"User's assigned branch not found in system. Please contact administrator."
            );

        if (!string.IsNullOrEmpty(branchCode))
        {
            if (!_currentUser.CanAccessBranchCode(branchCode))
                throw new UnauthorizedException("User does not have access to the specified branch");
            return branchCode;
        }

        return _currentUser.BranchCodes[0];
    }
}
