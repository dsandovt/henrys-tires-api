using HenryTires.Inventory.Application.Common;
using HenryTires.Inventory.Application.Ports;
using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Domain.Enums;
using MongoDB.Bson;

namespace HenryTires.Inventory.Application.UseCases.Sales;

/// <summary>
/// Service for managing Sales with clean Goods vs Services separation.
///
/// CORE RULES:
/// - Sale can contain BOTH Goods and Services
/// - Only Goods generate InventoryTransaction (OUT)
/// - Services are revenue-only, no inventory impact
/// - ONE Sale may generate ONE InventoryTransaction with ONLY goods lines
/// </summary>
public class SaleService
{
    private readonly ISaleRepository _saleRepository;
    private readonly IItemRepository _itemRepository;
    private readonly IInventoryTransactionRepository _transactionRepository;
    private readonly IInventorySummaryRepository _summaryRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly IMongoUnitOfWork _unitOfWork;

    public SaleService(
        ISaleRepository saleRepository,
        IItemRepository itemRepository,
        IInventoryTransactionRepository transactionRepository,
        IInventorySummaryRepository summaryRepository,
        ICurrentUser currentUser,
        IClock clock,
        IMongoUnitOfWork unitOfWork)
    {
        _saleRepository = saleRepository;
        _itemRepository = itemRepository;
        _transactionRepository = transactionRepository;
        _summaryRepository = summaryRepository;
        _currentUser = currentUser;
        _clock = clock;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Create a new Sale (Draft status).
    /// Validates items and stock availability for Goods.
    /// Does NOT post to inventory yet.
    /// </summary>
    public async Task<Sale> CreateSaleAsync(
        string? branchId,
        DateTime saleDateUtc,
        List<SaleLine> lines,
        string? customerName,
        string? customerPhone,
        string? notes)
    {
        // Validate branch access
        var validBranchId = _currentUser.BranchId
            ?? branchId
            ?? throw new ValidationException("Branch is required");

        // Validate all items exist and enrich lines
        foreach (var line in lines)
        {
            var item = await _itemRepository.GetByIdAsync(line.ItemId)
                ?? throw new NotFoundException($"Item {line.ItemId} not found");

            if (item.IsDeleted)
                throw new ValidationException($"Item {item.ItemCode} is deleted");

            // Ensure line has correct classification from item
            if (line.Classification != item.Classification)
                throw new ValidationException($"Line classification mismatch for item {item.ItemCode}");

            // Validate Condition is set for Goods
            if (item.Classification == Classification.Good && !line.Condition.HasValue)
                throw new ValidationException($"Condition is required for Good: {item.ItemCode}");

            // Validate Condition is NOT set for Services
            if (item.Classification == Classification.Service && line.Condition.HasValue)
                throw new ValidationException($"Condition must not be set for Service: {item.ItemCode}");
        }

        // Check stock availability for Goods ONLY
        var goodsLines = lines.Where(l => l.Classification == Classification.Good).ToList();
        foreach (var line in goodsLines)
        {
            var summary = await _summaryRepository.GetByKeyAsync(
                _currentUser.BranchCode!,
                line.ItemCode);

            if (summary == null)
                throw new BusinessException($"No stock found for {line.ItemCode} in this branch");

            var entry = summary.Entries.FirstOrDefault(e => e.Condition == line.Condition!.Value);
            if (entry == null || entry.OnHand < line.Quantity)
            {
                var available = entry?.OnHand ?? 0;
                throw new BusinessException(
                    $"Insufficient stock for {line.ItemCode} ({line.Condition}). " +
                    $"Available: {available}, Requested: {line.Quantity}");
            }
        }

        // Generate Sale Number
        var saleNumber = $"SALE-{_clock.UtcNow:yyyyMMdd}-{ObjectId.GenerateNewId().ToString()[..8].ToUpper()}";

        var sale = new Sale
        {
            Id = ObjectId.GenerateNewId().ToString(),
            SaleNumber = saleNumber,
            BranchId = validBranchId,
            SaleDateUtc = saleDateUtc,
            Lines = lines,
            CustomerName = customerName,
            CustomerPhone = customerPhone,
            Notes = notes,
            Status = TransactionStatus.Draft,
            CreatedAtUtc = _clock.UtcNow,
            CreatedBy = _currentUser.Username,
            ModifiedAtUtc = _clock.UtcNow,
            ModifiedBy = _currentUser.Username
        };

        await _saleRepository.CreateAsync(sale);
        return sale;
    }

    /// <summary>
    /// Post Sale to inventory.
    /// - Generates InventoryTransaction (OUT) for Goods ONLY
    /// - Services are ignored (revenue only)
    /// - Uses MongoDB transaction for atomicity
    /// </summary>
    public async Task<Sale> PostSaleAsync(string saleId)
    {
        using var session = await _unitOfWork.StartSessionAsync();

        try
        {
            session.StartTransaction();

            var sale = await _saleRepository.GetByIdAsync(saleId)
                ?? throw new NotFoundException($"Sale {saleId} not found");

            if (sale.Status != TransactionStatus.Draft)
                throw new BusinessException($"Sale {sale.SaleNumber} is already {sale.Status}");

            // Filter ONLY Goods for inventory transaction
            var goodsLines = sale.Lines
                .Where(l => l.Classification == Classification.Good)
                .ToList();

            string? inventoryTransactionId = null;

            // Create InventoryTransaction ONLY if there are Goods
            if (goodsLines.Any())
            {
                var transactionNumber = $"OUT-{_clock.UtcNow:yyyyMMdd}-{ObjectId.GenerateNewId().ToString()[..8].ToUpper()}";

                var inventoryLines = goodsLines.Select(line => new InventoryTransactionLine
                {
                    LineId = ObjectId.GenerateNewId().ToString(),
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
                    ExecutedAtUtc = _clock.UtcNow
                }).ToList();

                var inventoryTransaction = new InventoryTransaction
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    TransactionNumber = transactionNumber,
                    BranchCode = _currentUser.BranchCode!,
                    Type = TransactionType.Out,
                    Status = TransactionStatus.Draft,
                    TransactionDateUtc = sale.SaleDateUtc,
                    Lines = inventoryLines,
                    Notes = $"Sale: {sale.SaleNumber}",
                    CreatedAtUtc = _clock.UtcNow,
                    CreatedBy = _currentUser.Username,
                    ModifiedAtUtc = _clock.UtcNow,
                    ModifiedBy = _currentUser.Username
                };

                // Create and commit inventory transaction
                await _transactionRepository.CreateAsync(inventoryTransaction);

                // Commit inventory (update summaries)
                await CommitInventoryTransactionAsync(inventoryTransaction, session);

                inventoryTransactionId = inventoryTransaction.Id;

                // Link inventory transaction to goods lines
                foreach (var goodLine in goodsLines)
                {
                    goodLine.InventoryTransactionId = inventoryTransactionId;
                }
            }

            // Update Sale status
            sale.Status = TransactionStatus.Committed;
            sale.PostedAtUtc = _clock.UtcNow;
            sale.PostedBy = _currentUser.Username;
            sale.ModifiedAtUtc = _clock.UtcNow;
            sale.ModifiedBy = _currentUser.Username;

            await _saleRepository.UpdateAsync(sale);

            await session.CommitTransactionAsync();
            return sale;
        }
        catch
        {
            await session.AbortTransactionAsync();
            throw;
        }
    }

    /// <summary>
    /// Commit InventoryTransaction by updating inventory summaries.
    /// This is the same logic from NewTransactionService but isolated here.
    /// </summary>
    private async Task CommitInventoryTransactionAsync(
        InventoryTransaction transaction,
        MongoDB.Driver.IClientSessionHandle session)
    {
        // Update inventory summaries
        foreach (var line in transaction.Lines)
        {
            var summary = await _summaryRepository.GetByKeyAsync(
                transaction.BranchCode,
                line.ItemCode,
                session);

            if (summary == null)
            {
                // Create new summary
                var initialQuantity = transaction.Type == TransactionType.In ? line.Quantity : -line.Quantity;
                summary = new InventorySummary
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    BranchCode = transaction.BranchCode,
                    ItemCode = line.ItemCode,
                    Entries = new List<InventoryEntry>
                    {
                        new()
                        {
                            Condition = line.Condition,
                            OnHand = initialQuantity,
                            Reserved = 0,
                            LatestEntryDateUtc = transaction.TransactionDateUtc
                        }
                    },
                    OnHandTotal = initialQuantity,
                    ReservedTotal = 0,
                    Version = 1,
                    UpdatedAtUtc = _clock.UtcNow
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
                        LatestEntryDateUtc = transaction.TransactionDateUtc
                    };
                    summary.Entries.Add(entry);
                }

                // Update quantity based on transaction type
                var delta = transaction.Type == TransactionType.In ? line.Quantity : -line.Quantity;
                entry.OnHand += delta;
                entry.LatestEntryDateUtc = transaction.TransactionDateUtc;

                summary.Version++;
                summary.UpdatedAtUtc = _clock.UtcNow;
            }

            // Recalculate totals
            summary.OnHandTotal = summary.Entries.Sum(e => e.OnHand);
            summary.ReservedTotal = summary.Entries.Sum(e => e.Reserved);

            await _summaryRepository.UpsertAsync(summary, session);
        }

        // Mark transaction as committed
        transaction.Status = TransactionStatus.Committed;
        transaction.CommittedAtUtc = _clock.UtcNow;
        transaction.CommittedBy = _currentUser.Username;
        transaction.ModifiedAtUtc = _clock.UtcNow;
        transaction.ModifiedBy = _currentUser.Username;

        await _transactionRepository.UpdateAsync(transaction, session);
    }

    public async Task<Sale?> GetSaleByIdAsync(string saleId)
    {
        return await _saleRepository.GetByIdAsync(saleId);
    }

    public async Task<IEnumerable<Sale>> GetSalesByBranchAndDateRangeAsync(string branchId, DateTime from, DateTime to)
    {
        return await _saleRepository.GetByBranchAndDateRangeAsync(branchId, from, to);
    }

    public async Task<IEnumerable<Sale>> GetSalesByDateRangeAsync(DateTime from, DateTime to)
    {
        return await _saleRepository.GetByDateRangeAsync(from, to);
    }

    public async Task<IEnumerable<Sale>> SearchSalesAsync(string? branchId, DateTime? from, DateTime? to, int page, int pageSize)
    {
        return await _saleRepository.SearchAsync(branchId, from, to, page, pageSize);
    }

    public async Task<int> CountSalesAsync(string? branchId, DateTime? from, DateTime? to)
    {
        return await _saleRepository.CountAsync(branchId, from, to);
    }
}
