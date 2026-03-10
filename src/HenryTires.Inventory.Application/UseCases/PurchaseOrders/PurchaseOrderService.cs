using HenryTires.Inventory.Application.Common;
using HenryTires.Inventory.Application.DTOs;
using HenryTires.Inventory.Application.Ports;
using HenryTires.Inventory.Application.Ports.Inbound;
using HenryTires.Inventory.Application.Ports.Outbound;
using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Domain.Enums;
using HenryTires.Inventory.Domain.ValueObjects;

namespace HenryTires.Inventory.Application.UseCases.PurchaseOrders;

public class PurchaseOrderService : IPurchaseOrderService
{
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IItemRepository _itemRepository;
    private readonly IInventoryTransactionRepository _transactionRepository;
    private readonly IInventorySummaryRepository _summaryRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityGenerator _identityGenerator;
    private readonly ISequenceGenerator _sequenceGenerator;

    public PurchaseOrderService(
        IPurchaseOrderRepository purchaseOrderRepository,
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
        _purchaseOrderRepository = purchaseOrderRepository;
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

    public async Task<PurchaseOrderDto> CreatePurchaseOrderAsync(CreatePurchaseOrderDto request)
    {
        var branchCode = ValidateBranchAccess(request.BranchCode);

        var branch = await _branchRepository.GetByCodeAsync(branchCode)
            ?? throw new NotFoundException($"Branch {branchCode} not found");

        var lines = new List<PurchaseOrderLine>();
        foreach (var lineRequest in request.Lines)
        {
            var item = await _itemRepository.GetByItemCodeAsync(lineRequest.ItemCode)
                ?? throw new NotFoundException($"Item with code '{lineRequest.ItemCode}' not found");

            if (item.IsDeleted)
                throw new ValidationException($"Item '{lineRequest.ItemCode}' is deleted");

            if (!Enum.TryParse<ItemCondition>(lineRequest.Condition, true, out var condition))
                throw new ValidationException($"Invalid condition: {lineRequest.Condition}");

            if (lineRequest.Quantity <= 0)
                throw new ValidationException("Quantity must be greater than zero");

            lines.Add(new PurchaseOrderLine
            {
                LineId = _identityGenerator.GenerateId(),
                ItemReference = lineRequest.ItemReference,
                ItemCode = lineRequest.ItemCode,
                Condition = condition,
                Quantity = lineRequest.Quantity,
                UnitPrice = lineRequest.UnitPrice,
                Currency = lineRequest.Currency,
            });
        }

        var sequenceName = $"purchase-order-{branch.Code}";
        var sequence = await _sequenceGenerator.GetNextSequenceAsync(sequenceName);
        var poNumber = $"{branch.Code}-PO-{sequence:D7}";

        var userLite = _currentUser.ToUserLite();

        var purchaseOrder = new PurchaseOrder
        {
            Id = _identityGenerator.GenerateId(),
            Number = poNumber,
            BranchReference = branch.Id,
            BranchCode = branch.Code,
            OrderDateUtc = request.OrderDateUtc ?? _clock.UtcNow,
            Lines = lines,
            SupplierName = request.SupplierName,
            Notes = request.Notes,
            Status = PurchaseOrderStatus.Draft,
            StatusHistory = new List<StatusHistoryEntry<PurchaseOrderStatus>>
            {
                new()
                {
                    Date = _clock.UtcNow,
                    Status = PurchaseOrderStatus.Draft,
                    User = userLite,
                },
            },
            CreatedAtUtc = _clock.UtcNow,
            CreatedBy = _currentUser.Username,
        };

        await _purchaseOrderRepository.CreateAsync(purchaseOrder);
        return MapToDto(purchaseOrder);
    }

    public async Task<PurchaseOrderDto> ReceivePurchaseOrderAsync(string purchaseOrderId)
    {
        using var scope = await _unitOfWork.BeginTransactionAsync();
        try
        {
            var po = await _purchaseOrderRepository.GetByIdAsync(purchaseOrderId)
                ?? throw new NotFoundException($"Purchase order '{purchaseOrderId}' not found");

            if (po.Status != PurchaseOrderStatus.Draft)
                throw new BusinessException($"Purchase order {po.Number} is already {po.Status}");

            var inventoryLines = po.Lines.Select(line => new InventoryTransactionLine
            {
                LineId = _identityGenerator.GenerateId(),
                ItemCode = line.ItemCode,
                Condition = line.Condition,
                Quantity = line.Quantity,
            }).ToList();

            var userLite = _currentUser.ToUserLite();
            var now = _clock.UtcNow;

            var inventoryTransaction = new InventoryTransaction
            {
                Id = _identityGenerator.GenerateId(),
                BranchReference = po.BranchReference,
                BranchCode = po.BranchCode,
                Initiator = new EntityKey
                {
                    Reference = po.Id,
                    ReferenceNumber = po.Number,
                    EntityDefinitionCode = InitiatorType.PurchaseOrder,
                },
                Status = InventoryTransactionStatus.Draft,
                TransactionDateUtc = now,
                Lines = inventoryLines,
                Notes = $"PurchaseOrder: {po.Number}",
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

            // Commit inventory transaction — increase stock per line
            inventoryTransaction.Commit(userLite, now);

            foreach (var line in inventoryTransaction.Lines)
            {
                var summary = await _summaryRepository.GetByKeyAsync(
                    inventoryTransaction.BranchReference,
                    line.ItemCode,
                    scope
                );

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
                        UpdatedAtUtc = _clock.UtcNow,
                    };
                }

                summary.IncreaseStock(line.ItemCode, line.Condition, line.Quantity, _clock.UtcNow);
                summary.UpdatedAtUtc = _clock.UtcNow;

                await _summaryRepository.UpsertWithVersionCheckAsync(summary, scope);
            }

            await _transactionRepository.UpdateAsync(inventoryTransaction, scope);

            // Update PO status
            po.Status = PurchaseOrderStatus.Received;
            po.StatusHistory.Add(new StatusHistoryEntry<PurchaseOrderStatus>
            {
                Date = now,
                Status = PurchaseOrderStatus.Received,
                User = userLite,
            });
            po.ModifiedAtUtc = now;
            po.ModifiedBy = _currentUser.Username;

            await _purchaseOrderRepository.UpdateAsync(po);

            await scope.CommitAsync();
            return MapToDto(po);
        }
        catch
        {
            await scope.RollbackAsync();
            throw;
        }
    }

    public async Task<PurchaseOrderDto> CancelPurchaseOrderAsync(string purchaseOrderId)
    {
        var po = await _purchaseOrderRepository.GetByIdAsync(purchaseOrderId)
            ?? throw new NotFoundException($"Purchase order '{purchaseOrderId}' not found");

        if (po.Status != PurchaseOrderStatus.Draft)
            throw new BusinessException($"Only Draft purchase orders can be cancelled");

        var userLite = _currentUser.ToUserLite();
        po.Status = PurchaseOrderStatus.Cancelled;
        po.StatusHistory.Add(new StatusHistoryEntry<PurchaseOrderStatus>
        {
            Date = _clock.UtcNow,
            Status = PurchaseOrderStatus.Cancelled,
            User = userLite,
        });
        po.ModifiedAtUtc = _clock.UtcNow;
        po.ModifiedBy = _currentUser.Username;

        await _purchaseOrderRepository.UpdateAsync(po);
        return MapToDto(po);
    }

    public async Task<PurchaseOrderDto> GetPurchaseOrderByIdAsync(string id)
    {
        var po = await _purchaseOrderRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Purchase order '{id}' not found");

        return MapToDto(po);
    }

    public async Task<PaginatedResponse<PurchaseOrderDto>> SearchPurchaseOrdersAsync(
        string? branchReference,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize
    )
    {
        var validatedBranch = ValidateBranchAccessForQuery(branchReference);

        var items = await _purchaseOrderRepository.SearchAsync(validatedBranch, from, to, page, pageSize);
        var count = await _purchaseOrderRepository.CountAsync(validatedBranch, from, to);

        return new PaginatedResponse<PurchaseOrderDto>
        {
            Items = items.Select(MapToDto),
            TotalCount = count,
            Page = page,
            PageSize = pageSize,
        };
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

    private static PurchaseOrderDto MapToDto(PurchaseOrder po) => new()
    {
        Id = po.Id,
        Number = po.Number,
        BranchReference = po.BranchReference,
        BranchCode = po.BranchCode,
        OrderDateUtc = po.OrderDateUtc,
        Lines = po.Lines.Select(l => new PurchaseOrderLineDto
        {
            LineId = l.LineId,
            ItemReference = l.ItemReference,
            ItemCode = l.ItemCode,
            Condition = l.Condition.ToString(),
            Quantity = l.Quantity,
            UnitPrice = l.UnitPrice,
            Currency = l.Currency.ToString(),
            LineTotal = l.LineTotal,
        }).ToList(),
        SupplierName = po.SupplierName,
        Notes = po.Notes,
        Status = po.Status.ToString(),
        StatusHistory = po.StatusHistory.Select(sh => new StatusHistoryEntryDto
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
        CreatedAtUtc = po.CreatedAtUtc,
        CreatedBy = po.CreatedBy,
    };
}
