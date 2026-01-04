using HenryTires.Inventory.Application.Common;
using HenryTires.Inventory.Application.DTOs;
using HenryTires.Inventory.Application.Ports;
using HenryTires.Inventory.Application.Ports.Inbound;
using HenryTires.Inventory.Application.Ports.Outbound;
using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Domain.Enums;

namespace HenryTires.Inventory.Application.UseCases.Sales;

public class SaleService : ISaleService
{
    private readonly ISaleRepository _saleRepository;
    private readonly IItemRepository _itemRepository;
    private readonly IInventoryTransactionRepository _transactionRepository;
    private readonly IInventorySummaryRepository _summaryRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityGenerator _identityGenerator;
    private readonly ISequenceGenerator _sequenceGenerator;

    public SaleService(
        ISaleRepository saleRepository,
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
        _saleRepository = saleRepository;
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

    public async Task<Sale> CreateSaleAsync(CreateSaleRequest request)
    {
        // Validate branch access and get branch code
        string branchCode = ValidateBranchAccess(request.BranchCode);

        // Get branch entity to construct sale number
        Branch branch = await _branchRepository.GetByCodeAsync(branchCode)
            ?? throw new NotFoundException($"Branch {branchCode} not found");

        var lines = request
            .Lines.Select(l => new SaleLine
            {
                LineId = _identityGenerator.GenerateId(),
                ItemId = l.ItemId,
                ItemCode = l.ItemCode,
                Description = l.Description,
                Classification = l.Classification,
                Condition = l.Condition,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                Currency = l.Currency,
                IsTaxable = l.IsTaxable,
                AppliesShopFee = l.AppliesShopFee,
            })
            .ToList();

        foreach (var line in lines)
        {
            var item =
                await _itemRepository.GetByIdAsync(line.ItemId)
                ?? throw new NotFoundException($"Item {line.ItemId} not found");

            if (item.IsDeleted)
                throw new ValidationException($"Item {item.ItemCode} is deleted");

            if (line.Classification != item.Classification)
                throw new ValidationException(
                    $"Line classification mismatch for item {item.ItemCode}"
                );

            if (item.Classification == Classification.Good && !line.Condition.HasValue)
                throw new ValidationException($"Condition is required for Good: {item.ItemCode}");

            if (item.Classification == Classification.Service && line.Condition.HasValue)
                throw new ValidationException(
                    $"Condition must not be set for Service: {item.ItemCode}"
                );
        }

        var goodsLines = lines.Where(l => l.Classification == Classification.Good).ToList();
        foreach (var line in goodsLines)
        {
            var summary = await _summaryRepository.GetByKeyAsync(branch.Code, line.ItemCode);

            if (summary == null)
                throw new BusinessException($"No stock found for {line.ItemCode} in this branch");

            var entry = summary.Entries.FirstOrDefault(e => e.Condition == line.Condition!.Value);
            if (entry == null || entry.OnHand < line.Quantity)
            {
                var available = entry?.OnHand ?? 0;
                throw new BusinessException(
                    $"Insufficient stock for {line.ItemCode} ({line.Condition}). "
                        + $"Available: {available}, Requested: {line.Quantity}"
                );
            }
        }

        // Generate sequential sale number per branch: BRANCHCODE-0000001
        var sequenceName = $"sale-{branch.Code}";
        var sequence = await _sequenceGenerator.GetNextSequenceAsync(sequenceName);
        var saleNumber = $"{branch.Code}-{sequence:D7}"; // e.g., WARWICK-0025000

        var sale = new Sale
        {
            Id = _identityGenerator.GenerateId(),
            SaleNumber = saleNumber,
            BranchId = branch.Id,
            SaleDateUtc = request.SaleDateUtc,
            Lines = lines,
            CustomerName = request.CustomerName,
            CustomerPhone = request.CustomerPhone,
            Notes = request.Notes,
            PaymentMethod = request.PaymentMethod,
            Status = TransactionStatus.Draft,
            CreatedAtUtc = _clock.UtcNow,
            CreatedBy = _currentUser.Username,
            ModifiedAtUtc = _clock.UtcNow,
            ModifiedBy = _currentUser.Username,
        };

        await _saleRepository.CreateAsync(sale);
        return sale;
    }

    public async Task<Sale> PostSaleAsync(string saleId)
    {
        using var scope = await _unitOfWork.BeginTransactionAsync();

        try
        {
            var sale =
                await _saleRepository.GetByIdAsync(saleId)
                ?? throw new NotFoundException($"Sale {saleId} not found");

            if (sale.Status != TransactionStatus.Draft)
                throw new BusinessException($"Sale {sale.SaleNumber} is already {sale.Status}");

            // Get branch for the sale to use correct branch code
            var branch =
                await _branchRepository.GetByIdAsync(sale.BranchId)
                ?? throw new NotFoundException($"Branch {sale.BranchId} not found");

            var goodsLines = sale
                .Lines.Where(l => l.Classification == Classification.Good)
                .ToList();

            string? inventoryTransactionId = null;

            if (goodsLines.Any())
            {
                var transactionNumber =
                    $"OUT-{_clock.UtcNow:yyyyMMdd}-{_identityGenerator.GenerateId()[..8].ToUpper()}";

                var inventoryLines = goodsLines
                    .Select(line => new InventoryTransactionLine
                    {
                        LineId = _identityGenerator.GenerateId(),
                        ItemId = line.ItemId,
                        ItemCode = line.ItemCode,
                        Condition = line.Condition!.Value, // Safe because validated earlier
                        Quantity = line.Quantity,
                        UnitPrice = line.UnitPrice,
                        Currency = line.Currency,
                        PriceSource = PriceSource.Sale,
                        PriceSetByRole = _currentUser.Role.ToString(),
                        PriceSetByUser = _currentUser.Username,
                        LineTotal = line.LineTotal,
                        ExecutedAtUtc = _clock.UtcNow,
                    })
                    .ToList();

                var inventoryTransaction = new InventoryTransaction
                {
                    Id = _identityGenerator.GenerateId(),
                    TransactionNumber = transactionNumber,
                    BranchCode = branch.Code,
                    Type = TransactionType.Out,
                    Status = TransactionStatus.Draft,
                    TransactionDateUtc = sale.SaleDateUtc,
                    Lines = inventoryLines,
                    Notes = $"Sale: {sale.SaleNumber}",
                    CreatedAtUtc = _clock.UtcNow,
                    CreatedBy = _currentUser.Username,
                    ModifiedAtUtc = _clock.UtcNow,
                    ModifiedBy = _currentUser.Username,
                };

                await _transactionRepository.CreateAsync(inventoryTransaction);

                await CommitInventoryTransactionAsync(inventoryTransaction, scope);

                inventoryTransactionId = inventoryTransaction.Id;

                foreach (var goodLine in goodsLines)
                {
                    goodLine.InventoryTransactionId = inventoryTransactionId;
                }
            }

            sale.Status = TransactionStatus.Committed;
            sale.PostedAtUtc = _clock.UtcNow;
            sale.PostedBy = _currentUser.Username;
            sale.ModifiedAtUtc = _clock.UtcNow;
            sale.ModifiedBy = _currentUser.Username;

            await _saleRepository.UpdateAsync(sale);

            await scope.CommitAsync();
            return sale;
        }
        catch
        {
            await scope.RollbackAsync();
            throw;
        }
    }

    private async Task CommitInventoryTransactionAsync(
        InventoryTransaction transaction,
        ITransactionScope scope
    )
    {
        foreach (var line in transaction.Lines)
        {
            var summary = await _summaryRepository.GetByKeyAsync(
                transaction.BranchCode,
                line.ItemCode,
                scope
            );

            if (summary == null)
            {
                var initialQuantity =
                    transaction.Type == TransactionType.In ? line.Quantity : -line.Quantity;
                summary = new InventorySummary
                {
                    Id = _identityGenerator.GenerateId(),
                    BranchCode = transaction.BranchCode,
                    ItemCode = line.ItemCode,
                    Entries = new List<InventoryEntry>
                    {
                        new()
                        {
                            Condition = line.Condition,
                            OnHand = initialQuantity,
                            Reserved = 0,
                            LatestEntryDateUtc = transaction.TransactionDateUtc,
                        },
                    },
                    OnHandTotal = initialQuantity,
                    ReservedTotal = 0,
                    Version = 1,
                    UpdatedAtUtc = _clock.UtcNow,
                };
            }
            else
            {
                var entry = summary.Entries.FirstOrDefault(e => e.Condition == line.Condition);
                if (entry == null)
                {
                    entry = new InventoryEntry
                    {
                        Condition = line.Condition,
                        OnHand = 0,
                        Reserved = 0,
                        LatestEntryDateUtc = transaction.TransactionDateUtc,
                    };
                    summary.Entries.Add(entry);
                }

                var delta = transaction.Type == TransactionType.In ? line.Quantity : -line.Quantity;
                entry.OnHand += delta;
                entry.LatestEntryDateUtc = transaction.TransactionDateUtc;

                summary.Version++;
                summary.UpdatedAtUtc = _clock.UtcNow;
            }

            summary.OnHandTotal = summary.Entries.Sum(e => e.OnHand);
            summary.ReservedTotal = summary.Entries.Sum(e => e.Reserved);

            await _summaryRepository.UpsertAsync(summary, scope);
        }

        transaction.Status = TransactionStatus.Committed;
        transaction.CommittedAtUtc = _clock.UtcNow;
        transaction.CommittedBy = _currentUser.Username;
        transaction.ModifiedAtUtc = _clock.UtcNow;
        transaction.ModifiedBy = _currentUser.Username;

        await _transactionRepository.UpdateAsync(transaction, scope);
    }

    public async Task<Sale?> GetSaleByIdAsync(string saleId)
    {
        return await _saleRepository.GetByIdAsync(saleId);
    }

    public async Task<IEnumerable<Sale>> GetSalesByBranchAndDateRangeAsync(
        string branchId,
        DateTime from,
        DateTime to
    )
    {
        return await _saleRepository.GetByBranchAndDateRangeAsync(branchId, from, to);
    }

    public async Task<IEnumerable<Sale>> GetSalesByDateRangeAsync(DateTime from, DateTime to)
    {
        return await _saleRepository.GetByDateRangeAsync(from, to);
    }

    public async Task<IEnumerable<Sale>> SearchSalesAsync(
        string? branchId,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize
    )
    {
        return await _saleRepository.SearchAsync(branchId, from, to, page, pageSize);
    }

    public async Task<int> CountSalesAsync(string? branchId, DateTime? from, DateTime? to)
    {
        return await _saleRepository.CountAsync(branchId, from, to);
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
}
