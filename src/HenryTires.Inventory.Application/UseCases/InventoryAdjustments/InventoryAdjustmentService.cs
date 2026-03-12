using HenryTires.Inventory.Application.Common;
using HenryTires.Inventory.Application.DTOs;
using HenryTires.Inventory.Application.Ports;
using HenryTires.Inventory.Application.Ports.Inbound;
using HenryTires.Inventory.Application.Ports.Outbound;
using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Domain.Enums;
using HenryTires.Inventory.Domain.ValueObjects;

namespace HenryTires.Inventory.Application.UseCases.InventoryAdjustments;

public class InventoryAdjustmentService : IInventoryAdjustmentService
{
    private readonly IInventoryAdjustmentRepository _adjustmentRepository;
    private readonly IItemRepository _itemRepository;
    private readonly IInventoryTransactionRepository _transactionRepository;
    private readonly IInventorySummaryRepository _summaryRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityGenerator _identityGenerator;
    private readonly ISequenceGenerator _sequenceGenerator;

    public InventoryAdjustmentService(
        IInventoryAdjustmentRepository adjustmentRepository,
        IItemRepository itemRepository,
        IInventoryTransactionRepository transactionRepository,
        IInventorySummaryRepository summaryRepository,
        IBranchRepository branchRepository,
        ICurrentUser currentUser,
        IClock clock,
        IUnitOfWork unitOfWork,
        IIdentityGenerator identityGenerator,
        ISequenceGenerator sequenceGenerator
    )
    {
        _adjustmentRepository = adjustmentRepository;
        _itemRepository = itemRepository;
        _transactionRepository = transactionRepository;
        _summaryRepository = summaryRepository;
        _branchRepository = branchRepository;
        _currentUser = currentUser;
        _clock = clock;
        _unitOfWork = unitOfWork;
        _identityGenerator = identityGenerator;
        _sequenceGenerator = sequenceGenerator;
    }

    public async Task<InventoryAdjustmentDto> CreateBranchTransferAsync(CreateBranchTransferDto request)
    {
        var originBranch = await _branchRepository.GetByCodeAsync(request.OriginBranchCode)
            ?? throw new NotFoundException($"Origin branch '{request.OriginBranchCode}' not found");

        var destinationBranch = await _branchRepository.GetByCodeAsync(request.DestinationBranchCode)
            ?? throw new NotFoundException($"Destination branch '{request.DestinationBranchCode}' not found");

        if (originBranch.Id == destinationBranch.Id)
            throw new ValidationException("Origin and destination branches must be different");

        var lines = await BuildLines(request.Lines);

        // Validate stock at origin branch
        foreach (var line in lines)
        {
            var summary = await _summaryRepository.GetByKeyAsync(originBranch.Id, line.ItemCode);

            if (summary == null)
                throw new BusinessException($"No stock found for {line.ItemCode} in origin branch ({originBranch.Code})");

            var entry = summary.Entries.FirstOrDefault(e => e.Condition == line.Condition);
            if (entry == null || entry.OnHand < line.Quantity)
            {
                var available = entry?.OnHand ?? 0;
                throw new BusinessException(
                    $"Insufficient stock for {line.ItemCode} ({line.Condition}) in origin branch ({originBranch.Code}). "
                        + $"Available: {available}, Requested: {line.Quantity}"
                );
            }
        }

        var sequenceName = $"adjustment-{originBranch.Code}";
        var sequence = await _sequenceGenerator.GetNextSequenceAsync(sequenceName);
        var number = $"{originBranch.Code}-ADJ-{sequence:D7}";

        var userLite = _currentUser.ToUserLite();
        var now = _clock.UtcNow;

        var adjustment = new InventoryAdjustment
        {
            Id = _identityGenerator.GenerateId(),
            Number = number,
            AdjustmentType = AdjustmentType.BranchTransfer,
            Status = InventoryAdjustmentStatus.Draft,
            StatusHistory = new List<StatusHistoryEntry<InventoryAdjustmentStatus>>
            {
                new()
                {
                    Date = now,
                    Status = InventoryAdjustmentStatus.Draft,
                    User = userLite,
                },
            },
            AdjustmentDateUtc = now,
            Notes = request.Notes,
            OriginBranchReference = originBranch.Id,
            OriginBranchCode = originBranch.Code,
            DestinationBranchReference = destinationBranch.Id,
            DestinationBranchCode = destinationBranch.Code,
            Lines = lines,
            CreatedAtUtc = now,
            CreatedBy = _currentUser.Username,
        };

        await _adjustmentRepository.CreateAsync(adjustment);
        return MapToDto(adjustment);
    }

    public async Task<InventoryAdjustmentDto> CreateStockCorrectionAsync(CreateStockCorrectionDto request)
    {
        var branchCode = ValidateBranchAccess(request.BranchCode);

        var branch = await _branchRepository.GetByCodeAsync(branchCode)
            ?? throw new NotFoundException($"Branch '{branchCode}' not found");

        var lines = await BuildLines(request.Lines);

        if (request.Direction == CorrectionDirection.Decrease)
        {
            foreach (var line in lines)
            {
                var summary = await _summaryRepository.GetByKeyAsync(branch.Id, line.ItemCode);

                if (summary == null)
                    throw new BusinessException($"No stock found for {line.ItemCode} in branch ({branch.Code}). Cannot decrease.");

                var entry = summary.Entries.FirstOrDefault(e => e.Condition == line.Condition);
                if (entry == null || entry.OnHand < line.Quantity)
                {
                    var available = entry?.OnHand ?? 0;
                    throw new BusinessException(
                        $"Insufficient stock for {line.ItemCode} ({line.Condition}) in branch ({branch.Code}). "
                            + $"Available: {available}, Requested: {line.Quantity}"
                    );
                }
            }
        }

        var sequenceName = $"adjustment-{branch.Code}";
        var sequence = await _sequenceGenerator.GetNextSequenceAsync(sequenceName);
        var number = $"{branch.Code}-ADJ-{sequence:D7}";

        var userLite = _currentUser.ToUserLite();
        var now = _clock.UtcNow;

        var adjustment = new InventoryAdjustment
        {
            Id = _identityGenerator.GenerateId(),
            Number = number,
            AdjustmentType = AdjustmentType.StockCorrection,
            Status = InventoryAdjustmentStatus.Draft,
            StatusHistory = new List<StatusHistoryEntry<InventoryAdjustmentStatus>>
            {
                new()
                {
                    Date = now,
                    Status = InventoryAdjustmentStatus.Draft,
                    User = userLite,
                },
            },
            AdjustmentDateUtc = now,
            Notes = request.Notes,
            BranchReference = branch.Id,
            BranchCode = branch.Code,
            Direction = request.Direction,
            Lines = lines,
            CreatedAtUtc = now,
            CreatedBy = _currentUser.Username,
        };

        await _adjustmentRepository.CreateAsync(adjustment);
        return MapToDto(adjustment);
    }

    public async Task<InventoryAdjustmentDto> CommitAdjustmentAsync(string id)
    {
        using var scope = await _unitOfWork.BeginTransactionAsync();
        try
        {
            var adjustment = await _adjustmentRepository.GetByIdAsync(id)
                ?? throw new NotFoundException($"Inventory adjustment '{id}' not found");

            if (adjustment.Status != InventoryAdjustmentStatus.Draft)
                throw new BusinessException($"Adjustment {adjustment.Number} is already {adjustment.Status}");

            var userLite = _currentUser.ToUserLite();
            var now = _clock.UtcNow;

            if (adjustment.AdjustmentType == AdjustmentType.BranchTransfer)
            {
                await CommitBranchTransferAsync(adjustment, userLite, now, scope);
            }
            else
            {
                await CommitStockCorrectionAsync(adjustment, userLite, now, scope);
            }

            adjustment.Status = InventoryAdjustmentStatus.Committed;
            adjustment.StatusHistory.Add(new StatusHistoryEntry<InventoryAdjustmentStatus>
            {
                Date = now,
                Status = InventoryAdjustmentStatus.Committed,
                User = userLite,
            });
            adjustment.ModifiedAtUtc = now;
            adjustment.ModifiedBy = _currentUser.Username;

            await _adjustmentRepository.UpdateAsync(adjustment);

            await scope.CommitAsync();
            return MapToDto(adjustment);
        }
        catch
        {
            await scope.RollbackAsync();
            throw;
        }
    }

    public async Task<InventoryAdjustmentDto> CancelAdjustmentAsync(string id)
    {
        var adjustment = await _adjustmentRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Inventory adjustment '{id}' not found");

        if (adjustment.Status != InventoryAdjustmentStatus.Draft)
            throw new BusinessException("Only Draft adjustments can be cancelled");

        var userLite = _currentUser.ToUserLite();
        adjustment.Status = InventoryAdjustmentStatus.Cancelled;
        adjustment.StatusHistory.Add(new StatusHistoryEntry<InventoryAdjustmentStatus>
        {
            Date = _clock.UtcNow,
            Status = InventoryAdjustmentStatus.Cancelled,
            User = userLite,
        });
        adjustment.ModifiedAtUtc = _clock.UtcNow;
        adjustment.ModifiedBy = _currentUser.Username;

        await _adjustmentRepository.UpdateAsync(adjustment);
        return MapToDto(adjustment);
    }

    public async Task<InventoryAdjustmentDto> GetByIdAsync(string id)
    {
        var adjustment = await _adjustmentRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Inventory adjustment '{id}' not found");

        return MapToDto(adjustment);
    }

    public async Task<PaginatedResponse<InventoryAdjustmentDto>> SearchAsync(
        string? branchReference,
        AdjustmentType? adjustmentType,
        InventoryAdjustmentStatus? status,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize
    )
    {
        var validatedBranch = ValidateBranchAccessForQuery(branchReference);

        var items = await _adjustmentRepository.SearchAsync(validatedBranch, adjustmentType, status, from, to, page, pageSize);
        var count = await _adjustmentRepository.CountAsync(validatedBranch, adjustmentType, status, from, to);

        return new PaginatedResponse<InventoryAdjustmentDto>
        {
            Items = items.Select(MapToDto),
            TotalCount = count,
            Page = page,
            PageSize = pageSize,
        };
    }

    private async Task CommitBranchTransferAsync(
        InventoryAdjustment adjustment,
        UserLite userLite,
        DateTime now,
        ITransactionScope scope
    )
    {
        var originInventoryLines = adjustment.Lines.Select(line => new InventoryTransactionLine
        {
            LineId = _identityGenerator.GenerateId(),
            ItemCode = line.ItemCode,
            Condition = line.Condition,
            Quantity = line.Quantity,
        }).ToList();

        var destInventoryLines = adjustment.Lines.Select(line => new InventoryTransactionLine
        {
            LineId = _identityGenerator.GenerateId(),
            ItemCode = line.ItemCode,
            Condition = line.Condition,
            Quantity = line.Quantity,
        }).ToList();

        // Create BOTH inventory transaction documents BEFORE any transactional reads
        // so they are visible within the transaction snapshot.
        var originTransaction = new InventoryTransaction
        {
            Id = _identityGenerator.GenerateId(),
            BranchReference = adjustment.OriginBranchReference!,
            BranchCode = adjustment.OriginBranchCode!,
            Initiator = new EntityKey
            {
                Reference = adjustment.Id,
                ReferenceNumber = adjustment.Number,
                EntityDefinitionCode = InitiatorType.BranchTransfer,
            },
            Status = InventoryTransactionStatus.Draft,
            TransactionDateUtc = now,
            Lines = originInventoryLines,
            Notes = $"BranchTransfer (out): {adjustment.Number}",
            StatusHistory = new List<StatusHistoryEntry<InventoryTransactionStatus>>
            {
                new()
                {
                    Date = now,
                    Status = InventoryTransactionStatus.Draft,
                    User = userLite,
                },
            },
            CreatedAtUtc = now,
            CreatedBy = _currentUser.Username,
        };

        var destTransaction = new InventoryTransaction
        {
            Id = _identityGenerator.GenerateId(),
            BranchReference = adjustment.DestinationBranchReference!,
            BranchCode = adjustment.DestinationBranchCode!,
            Initiator = new EntityKey
            {
                Reference = adjustment.Id,
                ReferenceNumber = adjustment.Number,
                EntityDefinitionCode = InitiatorType.BranchTransfer,
            },
            Status = InventoryTransactionStatus.Draft,
            TransactionDateUtc = now,
            Lines = destInventoryLines,
            Notes = $"BranchTransfer (in): {adjustment.Number}",
            StatusHistory = new List<StatusHistoryEntry<InventoryTransactionStatus>>
            {
                new()
                {
                    Date = now,
                    Status = InventoryTransactionStatus.Draft,
                    User = userLite,
                },
            },
            CreatedAtUtc = now,
            CreatedBy = _currentUser.Username,
        };

        await _transactionRepository.CreateAsync(originTransaction);
        await _transactionRepository.CreateAsync(destTransaction);

        // Now commit both and update stock within the transaction scope
        originTransaction.Commit(userLite, now);

        foreach (var line in originTransaction.Lines)
        {
            var summary = await _summaryRepository.GetByKeyAsync(
                originTransaction.BranchReference,
                line.ItemCode,
                scope
            );

            if (summary == null)
                throw new BusinessException($"No stock found for {line.ItemCode} in origin branch ({adjustment.OriginBranchCode})");

            var entry = summary.Entries.FirstOrDefault(e => e.Condition == line.Condition);
            if (entry == null || entry.OnHand < line.Quantity)
            {
                var available = entry?.OnHand ?? 0;
                throw new BusinessException(
                    $"Insufficient stock for {line.ItemCode} ({line.Condition}) in origin branch ({adjustment.OriginBranchCode}). "
                        + $"Available: {available}, Requested: {line.Quantity}"
                );
            }

            summary.DecreaseStock(line.ItemCode, line.Condition, line.Quantity, now);
            summary.UpdatedAtUtc = now;
            await _summaryRepository.UpsertWithVersionCheckAsync(summary, scope);
        }

        await _transactionRepository.UpdateAsync(originTransaction, scope);

        // Destination branch — increase stock
        destTransaction.Commit(userLite, now);

        foreach (var line in destTransaction.Lines)
        {
            var summary = await _summaryRepository.GetByKeyAsync(
                destTransaction.BranchReference,
                line.ItemCode,
                scope
            );

            if (summary == null)
            {
                summary = new InventorySummary
                {
                    Id = _identityGenerator.GenerateId(),
                    BranchReference = destTransaction.BranchReference,
                    BranchCode = destTransaction.BranchCode,
                    ItemCode = line.ItemCode,
                    Entries = new List<InventoryEntry>(),
                    OnHandTotal = 0,
                    ReservedTotal = 0,
                    Version = 0,
                    UpdatedAtUtc = now,
                };
            }

            summary.IncreaseStock(line.ItemCode, line.Condition, line.Quantity, now);
            summary.UpdatedAtUtc = now;
            await _summaryRepository.UpsertWithVersionCheckAsync(summary, scope);
        }

        await _transactionRepository.UpdateAsync(destTransaction, scope);
    }

    private async Task CommitStockCorrectionAsync(
        InventoryAdjustment adjustment,
        UserLite userLite,
        DateTime now,
        ITransactionScope scope
    )
    {
        var inventoryLines = adjustment.Lines.Select(line => new InventoryTransactionLine
        {
            LineId = _identityGenerator.GenerateId(),
            ItemCode = line.ItemCode,
            Condition = line.Condition,
            Quantity = line.Quantity,
        }).ToList();

        var inventoryTransaction = new InventoryTransaction
        {
            Id = _identityGenerator.GenerateId(),
            BranchReference = adjustment.BranchReference!,
            BranchCode = adjustment.BranchCode!,
            Initiator = new EntityKey
            {
                Reference = adjustment.Id,
                ReferenceNumber = adjustment.Number,
                EntityDefinitionCode = InitiatorType.StockAdjustment,
            },
            Status = InventoryTransactionStatus.Draft,
            TransactionDateUtc = now,
            Lines = inventoryLines,
            Notes = $"StockCorrection ({adjustment.Direction}): {adjustment.Number}",
            StatusHistory = new List<StatusHistoryEntry<InventoryTransactionStatus>>
            {
                new()
                {
                    Date = now,
                    Status = InventoryTransactionStatus.Draft,
                    User = userLite,
                },
            },
            CreatedAtUtc = now,
            CreatedBy = _currentUser.Username,
        };

        await _transactionRepository.CreateAsync(inventoryTransaction);
        inventoryTransaction.Commit(userLite, now);

        foreach (var line in inventoryTransaction.Lines)
        {
            var summary = await _summaryRepository.GetByKeyAsync(
                inventoryTransaction.BranchReference,
                line.ItemCode,
                scope
            );

            if (adjustment.Direction == CorrectionDirection.Increase)
            {
                if (summary == null)
                {
                    summary = new InventorySummary
                    {
                        Id = _identityGenerator.GenerateId(),
                        BranchReference = inventoryTransaction.BranchReference,
                        BranchCode = inventoryTransaction.BranchCode,
                        ItemCode = line.ItemCode,
                        Entries = new List<InventoryEntry>(),
                        OnHandTotal = 0,
                        ReservedTotal = 0,
                        Version = 0,
                        UpdatedAtUtc = now,
                    };
                }

                summary.IncreaseStock(line.ItemCode, line.Condition, line.Quantity, now);
            }
            else
            {
                if (summary == null)
                    throw new BusinessException($"No stock found for {line.ItemCode} in branch ({adjustment.BranchCode}). Cannot decrease.");

                var entry = summary.Entries.FirstOrDefault(e => e.Condition == line.Condition);
                if (entry == null || entry.OnHand < line.Quantity)
                {
                    var available = entry?.OnHand ?? 0;
                    throw new BusinessException(
                        $"Insufficient stock for {line.ItemCode} ({line.Condition}) in branch ({adjustment.BranchCode}). "
                            + $"Available: {available}, Requested: {line.Quantity}"
                    );
                }

                summary.DecreaseStock(line.ItemCode, line.Condition, line.Quantity, now);
            }

            summary.UpdatedAtUtc = now;
            await _summaryRepository.UpsertWithVersionCheckAsync(summary, scope);
        }

        await _transactionRepository.UpdateAsync(inventoryTransaction, scope);
    }

    private async Task<List<InventoryAdjustmentLine>> BuildLines(List<CreateAdjustmentLineDto> lineRequests)
    {
        var lines = new List<InventoryAdjustmentLine>();
        foreach (var lineRequest in lineRequests)
        {
            var item = await _itemRepository.GetByItemCodeAsync(lineRequest.ItemCode)
                ?? throw new NotFoundException($"Item with code '{lineRequest.ItemCode}' not found");

            if (item.IsDeleted)
                throw new ValidationException($"Item '{lineRequest.ItemCode}' is deleted");

            if (!Enum.TryParse<ItemCondition>(lineRequest.Condition, true, out var condition))
                throw new ValidationException($"Invalid condition: {lineRequest.Condition}");

            if (lineRequest.Quantity <= 0)
                throw new ValidationException("Quantity must be greater than zero");

            lines.Add(new InventoryAdjustmentLine
            {
                LineId = _identityGenerator.GenerateId(),
                ItemReference = lineRequest.ItemReference,
                ItemCode = lineRequest.ItemCode,
                Condition = condition,
                Quantity = lineRequest.Quantity,
                Notes = lineRequest.Notes,
            });
        }

        return lines;
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

    private string? ValidateBranchAccessForQuery(string? branchReference)
    {
        if (_currentUser.HasRole("ADMIN"))
            return branchReference;

        if (_currentUser.BranchReferences.Count == 0)
            throw new UnauthorizedException("User does not have an assigned branch");

        if (!string.IsNullOrEmpty(branchReference) && _currentUser.CanAccessBranch(branchReference))
            return branchReference;

        return _currentUser.BranchReferences.Count > 0 ? _currentUser.BranchReferences[0] : null;
    }

    private static InventoryAdjustmentDto MapToDto(InventoryAdjustment adjustment) => new()
    {
        Id = adjustment.Id,
        Number = adjustment.Number,
        AdjustmentType = adjustment.AdjustmentType.ToString(),
        Status = adjustment.Status.ToString(),
        StatusHistory = adjustment.StatusHistory.Select(sh => new StatusHistoryEntryDto
        {
            Date = sh.Date,
            Status = sh.Status.ToString(),
            User = new UserLiteDto
            {
                FirstName = sh.User.FirstName,
                MiddleName = sh.User.MiddleName,
                LastName = sh.User.LastName,
                SecondLastName = sh.User.SecondLastName,
                Username = sh.User.Username,
                Email = sh.User.Email,
            },
            Comment = sh.Comment,
        }).ToList(),
        AdjustmentDateUtc = adjustment.AdjustmentDateUtc,
        Notes = adjustment.Notes,
        OriginBranchReference = adjustment.OriginBranchReference,
        OriginBranchCode = adjustment.OriginBranchCode,
        DestinationBranchReference = adjustment.DestinationBranchReference,
        DestinationBranchCode = adjustment.DestinationBranchCode,
        BranchReference = adjustment.BranchReference,
        BranchCode = adjustment.BranchCode,
        Direction = adjustment.Direction?.ToString(),
        Lines = adjustment.Lines.Select(l => new InventoryAdjustmentLineDto
        {
            LineId = l.LineId,
            ItemReference = l.ItemReference,
            ItemCode = l.ItemCode,
            Condition = l.Condition.ToString(),
            Quantity = l.Quantity,
            Notes = l.Notes,
        }).ToList(),
        CreatedAtUtc = adjustment.CreatedAtUtc,
        CreatedBy = adjustment.CreatedBy,
        ModifiedAtUtc = adjustment.ModifiedAtUtc,
        ModifiedBy = adjustment.ModifiedBy,
    };
}
