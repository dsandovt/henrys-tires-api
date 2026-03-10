using HenryTires.Inventory.Application.Common;
using HenryTires.Inventory.Application.DTOs;
using HenryTires.Inventory.Application.Ports;
using HenryTires.Inventory.Application.Ports.Inbound;
using HenryTires.Inventory.Application.Ports.Outbound;
using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Domain.Enums;
using HenryTires.Inventory.Domain.ValueObjects;

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
        string branchCode = ValidateBranchAccess(request.BranchCode);

        Branch branch = await _branchRepository.GetByCodeAsync(branchCode)
            ?? throw new NotFoundException($"Branch {branchCode} not found");

        var lines = request
            .Lines.Select(l => new SaleLine
            {
                LineId = _identityGenerator.GenerateId(),
                ItemReference = l.ItemReference,
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
                await _itemRepository.GetByIdAsync(line.ItemReference)
                ?? throw new NotFoundException($"Item {line.ItemReference} not found");

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

        var goodsLinesToValidate = request.Lines
            .Where(l => l.Classification == Classification.Good)
            .Where(l => l.Condition != ItemCondition.New || !l.AllowWithoutStock)
            .ToList();
        foreach (var reqLine in goodsLinesToValidate)
        {
            var summary = await _summaryRepository.GetByKeyAsync(branch.Id, reqLine.ItemCode);

            if (summary == null)
                throw new BusinessException($"No stock found for {reqLine.ItemCode} in this branch");

            var entry = summary.Entries.FirstOrDefault(e => e.Condition == reqLine.Condition!.Value);
            if (entry == null || entry.OnHand < reqLine.Quantity)
            {
                var available = entry?.OnHand ?? 0;
                throw new BusinessException(
                    $"Insufficient stock for {reqLine.ItemCode} ({reqLine.Condition}). "
                        + $"Available: {available}, Requested: {reqLine.Quantity}"
                );
            }
        }

        // Validate payment details
        if (request.PaymentDetails != null)
        {
            foreach (var pd in request.PaymentDetails)
            {
                if (string.Equals(pd.Method, "Check", StringComparison.OrdinalIgnoreCase)
                    && string.IsNullOrWhiteSpace(pd.CheckNumber))
                {
                    throw new ValidationException("Check number is required when payment method is Check");
                }
            }
        }

        if (request.PaymentMethod == PaymentMethod.Split)
        {
            if (request.PaymentDetails == null || request.PaymentDetails.Count < 2)
                throw new ValidationException("Split payment requires at least two payment details");

            var saleTotal = lines.Sum(l => l.Quantity * l.UnitPrice);
            var detailsTotal = request.PaymentDetails.Sum(pd => pd.Amount);

            if (Math.Abs(saleTotal - detailsTotal) > 0.01m)
                throw new ValidationException(
                    $"Payment details total ({detailsTotal:F2}) must equal sale total ({saleTotal:F2})");
        }

        var sequenceName = $"sale-{branch.Code}";
        var sequence = await _sequenceGenerator.GetNextSequenceAsync(sequenceName);
        var saleNumber = $"{branch.Code}-SL-{sequence:D7}";

        var userLite = _currentUser.ToUserLite();

        var sale = new Sale
        {
            Id = _identityGenerator.GenerateId(),
            Number = saleNumber,
            BranchReference = branch.Id,
            BranchCode = branch.Code,
            SaleDateUtc = request.SaleDateUtc ?? _clock.UtcNow,
            Lines = lines,
            CustomerName = request.CustomerName,
            CustomerPhone = request.CustomerPhone,
            Notes = request.Notes,
            PaymentMethod = request.PaymentMethod,
            PaymentDetails = request.PaymentDetails?.Select(pd => new PaymentDetail
            {
                Method = Enum.TryParse<PaymentMethod>(pd.Method, true, out var m) ? m : Domain.Enums.PaymentMethod.Cash,
                Amount = pd.Amount,
                CheckNumber = pd.CheckNumber
            }).ToList(),
            Status = SaleStatus.Draft,
            StatusHistory = new List<StatusHistoryEntry<SaleStatus>>
            {
                new()
                {
                    Date = _clock.UtcNow,
                    Status = SaleStatus.Draft,
                    User = userLite,
                },
            },
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

            if (sale.Status != SaleStatus.Draft)
                throw new BusinessException($"Sale {sale.Number} is already {sale.Status}");

            var userLite = _currentUser.ToUserLite();
            var now = _clock.UtcNow;

            var goodsLines = sale
                .Lines.Where(l => l.Classification == Classification.Good)
                .ToList();

            if (goodsLines.Any())
            {
                var inventoryLines = goodsLines
                    .Select(line => new InventoryTransactionLine
                    {
                        LineId = _identityGenerator.GenerateId(),
                        ItemCode = line.ItemCode,
                        Condition = line.Condition!.Value,
                        Quantity = line.Quantity,
                    })
                    .ToList();

                var inventoryTransaction = new InventoryTransaction
                {
                    Id = _identityGenerator.GenerateId(),
                    BranchReference = sale.BranchReference,
                    BranchCode = sale.BranchCode,
                    Initiator = new EntityKey
                    {
                        Reference = sale.Id,
                        ReferenceNumber = sale.Number,
                        EntityDefinitionCode = InitiatorType.Sale,
                    },
                    Status = InventoryTransactionStatus.Draft,
                    TransactionDateUtc = sale.SaleDateUtc,
                    Lines = inventoryLines,
                    Notes = $"Sale: {sale.Number}",
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
                    ModifiedAtUtc = now,
                    ModifiedBy = _currentUser.Username,
                };

                await _transactionRepository.CreateAsync(inventoryTransaction);

                await CommitInventoryTransactionAsync(inventoryTransaction, userLite, scope);
            }

            sale.Status = SaleStatus.Committed;
            sale.StatusHistory.Add(new StatusHistoryEntry<SaleStatus>
            {
                Date = now,
                Status = SaleStatus.Committed,
                User = userLite,
            });
            sale.ModifiedAtUtc = now;
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
        UserLite userLite,
        ITransactionScope scope
    )
    {
        foreach (var line in transaction.Lines)
        {
            var summary = await _summaryRepository.GetByKeyAsync(
                transaction.BranchReference,
                line.ItemCode,
                scope
            );

            if (summary == null)
            {
                summary = new InventorySummary
                {
                    Id = _identityGenerator.GenerateId(),
                    BranchReference = transaction.BranchReference,
                    BranchCode = transaction.BranchCode,
                    ItemCode = line.ItemCode,
                    Entries = new List<InventoryEntry>(),
                    OnHandTotal = 0,
                    ReservedTotal = 0,
                    Version = 0,
                    UpdatedAtUtc = _clock.UtcNow,
                };
            }

            summary.DecreaseStock(line.ItemCode, line.Condition, line.Quantity, transaction.TransactionDateUtc);
            summary.UpdatedAtUtc = _clock.UtcNow;

            await _summaryRepository.UpsertAsync(summary, scope);
        }

        transaction.Commit(userLite, _clock.UtcNow);

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
