using HenryTires.Inventory.Application.DTOs;
using HenryTires.Inventory.Application.Ports;
using HenryTires.Inventory.Application.Ports.Outbound;
using HenryTires.Inventory.Domain.Enums;
using HenryTires.Inventory.Domain.ValueObjects;

namespace HenryTires.Inventory.Application.UseCases.Reports;

public interface IReportService
{
    Task<StockReportDto> GetStockReportAsync(string? branchId);
    Task<InvoiceDto> GetSaleInvoiceAsync(string saleId);
    Task<SalesReportDto> GetSalesReportAsync(
        string? branchReference,
        DateTime fromUtc,
        DateTime toUtc,
        string sortOrder = "desc"
    );
    Task<DailyCloseReportDto> GetDailyCloseReportAsync(
        string branchReference,
        DateTime dateUtc,
        string groupBy = "day",
        int timezoneOffsetMinutes = -300
    );
    Task<InventoryMovementsReportDto> GetInventoryMovementsAsync(
        DateTime? fromDate,
        DateTime? toDate,
        string? branchReference,
        string? initiatorType,
        string? status
    );
    Task<KardexReportDto> GetKardexAsync(
        string itemCode,
        string? condition,
        string? branchReference,
        DateTime? fromDate,
        DateTime? toDate
    );
    Task<SalesByVolumeReportDto> GetSalesByVolumeAsync(
        string? branchReference,
        DateTime fromUtc,
        DateTime toUtc
    );
}

public class ReportService : IReportService
{
    private readonly IInventorySummaryRepository _inventorySummaryRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IItemRepository _itemRepository;
    private readonly ISaleRepository _saleRepository;
    private readonly IInventoryTransactionRepository _transactionRepository;
    private readonly IClock _clock;
    private readonly ICompanyInfoProvider _companyInfoProvider;
    private readonly ICurrentUserService _currentUser;
    private const decimal DEFAULT_SALES_TAX_RATE = 0.07m;
    private const decimal DEFAULT_SHOP_FEE_RATE = 0.01m;

    public ReportService(
        IInventorySummaryRepository inventorySummaryRepository,
        IBranchRepository branchRepository,
        IItemRepository itemRepository,
        ISaleRepository saleRepository,
        IInventoryTransactionRepository transactionRepository,
        IClock clock,
        ICompanyInfoProvider companyInfoProvider,
        ICurrentUserService currentUser
    )
    {
        _inventorySummaryRepository = inventorySummaryRepository;
        _branchRepository = branchRepository;
        _itemRepository = itemRepository;
        _saleRepository = saleRepository;
        _transactionRepository = transactionRepository;
        _clock = clock;
        _companyInfoProvider = companyInfoProvider;
        _currentUser = currentUser;
    }

    public async Task<StockReportDto> GetStockReportAsync(string? branchId)
    {
        string? branchCode = null;
        string? branchName = null;
        if (!string.IsNullOrEmpty(branchId))
        {
            var branch = await _branchRepository.GetByIdAsync(branchId);
            if (branch != null)
            {
                branchCode = branch.Code;
                branchName = branch.Name;
            }
        }

        var summaries = await _inventorySummaryRepository.GetByBranchAsync(
            branchId,
            search: null,
            condition: null,
            page: 1,
            pageSize: 100000
        );

        var items = await _itemRepository.GetAllAsync(Classification.Good);
        var goodItems = items.ToDictionary(i => i.ItemCode, i => i);

        var rows = new List<StockReportRow>();
        foreach (var summary in summaries)
        {
            if (!goodItems.ContainsKey(summary.ItemCode))
                continue;

            var item = goodItems[summary.ItemCode];

            foreach (var entry in summary.Entries)
            {
                var available = entry.OnHand - entry.Reserved;
                rows.Add(new StockReportRow
                {
                    ItemCode = summary.ItemCode,
                    Description = item.Description,
                    Condition = entry.Condition.ToString(),
                    OnHand = entry.OnHand,
                    Reserved = entry.Reserved,
                    Available = available
                });
            }
        }

        var newRows = rows.Where(r => r.Condition == "New").ToList();
        var usedRows = rows.Where(r => r.Condition == "Used").ToList();

        var totals = new StockReportTotals
        {
            NewOnHand = newRows.Sum(r => r.OnHand),
            NewReserved = newRows.Sum(r => r.Reserved),
            NewAvailable = newRows.Sum(r => r.Available),
            UsedOnHand = usedRows.Sum(r => r.OnHand),
            UsedReserved = usedRows.Sum(r => r.Reserved),
            UsedAvailable = usedRows.Sum(r => r.Available),
            TotalOnHand = rows.Sum(r => r.OnHand),
            TotalReserved = rows.Sum(r => r.Reserved),
            TotalAvailable = rows.Sum(r => r.Available)
        };

        return new StockReportDto
        {
            GeneratedAtUtc = _clock.UtcNow,
            BranchCode = branchCode,
            BranchName = branchName,
            Rows = rows.OrderBy(r => r.ItemCode).ThenBy(r => r.Condition).ToList(),
            Totals = totals
        };
    }

    public async Task<InvoiceDto> GetSaleInvoiceAsync(string saleId)
    {
        var sale = await _saleRepository.GetByIdAsync(saleId)
            ?? throw new InvalidOperationException($"Sale with ID {saleId} not found");

        var branch = await _branchRepository.GetByCodeAsync(sale.BranchCode)
            ?? throw new InvalidOperationException($"Branch with code {sale.BranchCode} not found");

        var companyInfo = _companyInfoProvider.GetCompanyInfo(branch.Address, branch.Phone);

        var invoiceLines = sale.Lines.Select(line => new InvoiceLineDto
        {
            ItemCode = line.ItemCode,
            Description = line.Description,
            Condition = line.Condition?.ToString(),
            Quantity = line.Quantity,
            UnitPrice = line.UnitPrice,
            Currency = line.Currency.ToString(),
            LineTotal = line.LineTotal,
            IsTaxable = line.IsTaxable,
            AppliesShopFee = line.AppliesShopFee
        }).ToList();

        var totals = CalculateInvoiceTotals(invoiceLines);
        totals.AmountDue = totals.GrandTotal - totals.AmountPaid;

        return new InvoiceDto
        {
            CompanyInfo = companyInfo,
            InvoiceNumber = sale.Number,
            InvoiceDateUtc = sale.SaleDateUtc,
            BranchCode = branch.Code,
            BranchName = branch.Name,
            PaymentMethod = sale.PaymentDetails != null && sale.PaymentDetails.Any()
                ? string.Join(" / ", sale.PaymentDetails.Select(pd => $"{pd.Method} ${pd.Amount:N2}"))
                : sale.PaymentMethod.ToString(),
            PaymentDetails = sale.PaymentDetails?.Select(pd => new PaymentDetailDto
            {
                Method = pd.Method.ToString(),
                Amount = pd.Amount,
                CheckNumber = pd.CheckNumber
            }).ToList(),
            CustomerName = sale.CustomerName,
            CustomerNumber = null,
            CustomerPhone = sale.CustomerPhone,
            PONumber = null,
            ServiceRep = _currentUser.Username,
            Notes = sale.Notes,
            Lines = invoiceLines,
            Totals = totals,
            GeneratedAtUtc = _clock.UtcNow,
            DocumentType = "INVOICE"
        };
    }

    public async Task<SalesReportDto> GetSalesReportAsync(
        string? branchReference,
        DateTime fromUtc,
        DateTime toUtc,
        string sortOrder = "desc"
    )
    {
        // Authorization: non-admin can only see their branches
        var isAdmin = _currentUser.RoleCodes?.Contains("ADMIN") ?? false;

        string? branchRef = null;
        string? branchCode = null;
        string? branchName = null;

        if (!string.IsNullOrEmpty(branchReference) && branchReference != "ALL")
        {
            var branch = await _branchRepository.GetByIdAsync(branchReference);
            if (branch == null)
                throw new InvalidOperationException($"Branch not found: {branchReference}");

            if (!isAdmin && !(_currentUser.BranchReferences?.Contains(branchReference) ?? false))
                throw new UnauthorizedAccessException("You do not have access to this branch");

            branchRef = branch.Id;
            branchCode = branch.Code;
            branchName = branch.Name;
        }
        else if (!isAdmin)
        {
            // Non-admin with no specific branch — use first assigned
            branchRef = _currentUser.BranchReferences?.FirstOrDefault();
            if (branchRef != null)
            {
                var branch = await _branchRepository.GetByIdAsync(branchRef);
                branchCode = branch?.Code;
                branchName = branch?.Name;
            }
        }

        var sales = await _saleRepository.SearchAsync(branchRef, fromUtc, toUtc, 1, 100000);
        var committedSales = sales.Where(s => s.Status == SaleStatus.Committed).ToList();

        var allBranches = await _branchRepository.GetAllAsync();
        var branchMap = allBranches.ToDictionary(b => b.Code, b => b.Name);

        var rows = committedSales.Select(s =>
        {
            var linesSummary = string.Join(", ", s.Lines.Select(l =>
                $"{l.ItemCode} x{l.Quantity}"));
            var total = s.Lines.Sum(l => l.LineTotal);
            var currency = s.Lines.FirstOrDefault()?.Currency.ToString() ?? "USD";

            return new SalesReportRowDto
            {
                SaleNumber = s.Number,
                BranchCode = s.BranchCode,
                BranchName = branchMap.GetValueOrDefault(s.BranchCode, s.BranchCode),
                SaleDateUtc = s.SaleDateUtc,
                CustomerName = s.CustomerName,
                LineCount = s.Lines.Count,
                LinesSummary = linesSummary,
                PaymentMethod = s.PaymentMethod.ToString(),
                Total = total,
                Currency = currency,
                Status = s.Status.ToString()
            };
        }).ToList();

        rows = sortOrder.Equals("asc", StringComparison.OrdinalIgnoreCase)
            ? rows.OrderBy(r => r.SaleDateUtc).ToList()
            : rows.OrderByDescending(r => r.SaleDateUtc).ToList();

        var totals = new SalesReportTotalsDto
        {
            GrandTotal = rows.Sum(r => r.Total),
            TotalSales = rows.Count,
            TotalItems = committedSales.SelectMany(s => s.Lines).Sum(l => l.Quantity)
        };

        return new SalesReportDto
        {
            GeneratedAtUtc = _clock.UtcNow,
            FromDateUtc = fromUtc,
            ToDateUtc = toUtc,
            BranchCode = branchCode,
            BranchName = branchName,
            Rows = rows,
            Totals = totals,
            TotalCount = rows.Count
        };
    }

    public async Task<DailyCloseReportDto> GetDailyCloseReportAsync(
        string branchReference,
        DateTime dateUtc,
        string groupBy = "day",
        int timezoneOffsetMinutes = -300
    )
    {
        var isAdmin = _currentUser.RoleCodes?.Contains("ADMIN") ?? false;

        string? branchRef = null;
        string? branchCode = null;
        string? branchName = null;

        if (!string.IsNullOrEmpty(branchReference) && branchReference != "ALL")
        {
            var branch = await _branchRepository.GetByIdAsync(branchReference);
            if (branch == null)
                throw new InvalidOperationException($"Branch not found: {branchReference}");

            if (!isAdmin && !(_currentUser.BranchReferences?.Contains(branchReference) ?? false))
                throw new UnauthorizedAccessException("You do not have access to this branch");

            branchRef = branch.Id;
            branchCode = branch.Code;
            branchName = branch.Name;
        }
        else if (!isAdmin)
        {
            branchRef = _currentUser.BranchReferences?.FirstOrDefault();
            if (branchRef != null)
            {
                var branch = await _branchRepository.GetByIdAsync(branchRef);
                branchCode = branch?.Code;
                branchName = branch?.Name;
            }
        }

        // Convert local day boundaries to UTC using the provided timezone offset
        var dayStartUtc = dateUtc.Date.AddMinutes(-timezoneOffsetMinutes);
        var dayEndUtc = dateUtc.Date.AddDays(1).AddMinutes(-timezoneOffsetMinutes).AddTicks(-1);

        var sales = await _saleRepository.SearchAsync(branchRef, dayStartUtc, dayEndUtc, 1, 100000);
        var committedSales = sales.Where(s => s.Status == SaleStatus.Committed).ToList();

        var details = committedSales.Select(s =>
        {
            var total = s.Lines.Sum(l => l.LineTotal);
            var currency = s.Lines.FirstOrDefault()?.Currency.ToString() ?? "USD";
            var paymentLabel = s.PaymentDetails != null && s.PaymentDetails.Count > 1
                ? "Mixed"
                : s.PaymentMethod.ToString();

            return new DailyCloseDetailDto
            {
                SaleNumber = s.Number,
                SaleDateUtc = s.SaleDateUtc,
                CustomerName = s.CustomerName,
                LineCount = s.Lines.Count,
                PaymentMethod = paymentLabel,
                Total = total,
                Currency = currency
            };
        }).OrderBy(d => d.SaleDateUtc).ToList();

        var currency1 = details.FirstOrDefault()?.Currency ?? "USD";
        var totalAmount = details.Sum(d => d.Total);

        // Payment breakdown
        var paymentBreakdown = details
            .GroupBy(d => d.PaymentMethod)
            .Select(g => new PaymentBreakdownDto
            {
                PaymentMethod = g.Key,
                Count = g.Count(),
                Amount = g.Sum(d => d.Total)
            })
            .OrderByDescending(p => p.Amount)
            .ToList();

        var summary = new DailyCloseSummaryDto
        {
            TotalSalesCount = details.Count,
            TotalAmount = totalAmount,
            Currency = currency1,
            PaymentBreakdown = paymentBreakdown
        };

        // Hour grouping
        List<DailyCloseHourGroupDto>? hourGroups = null;
        if (groupBy.Equals("hour", StringComparison.OrdinalIgnoreCase))
        {
            hourGroups = details
                .GroupBy(d => d.SaleDateUtc.AddMinutes(timezoneOffsetMinutes).Hour) // Convert UTC to local hour
                .OrderBy(g => g.Key)
                .Select(g => new DailyCloseHourGroupDto
                {
                    Hour = g.Key,
                    HourLabel = $"{g.Key:D2}:00 - {g.Key:D2}:59",
                    Sales = g.ToList(),
                    HourTotal = g.Sum(d => d.Total),
                    HourCount = g.Count()
                })
                .ToList();
        }

        return new DailyCloseReportDto
        {
            GeneratedAtUtc = _clock.UtcNow,
            DateUtc = dateUtc,
            BranchCode = branchCode,
            BranchName = branchName,
            GroupBy = groupBy,
            Summary = summary,
            Details = details,
            HourGroups = hourGroups
        };
    }

    public async Task<InventoryMovementsReportDto> GetInventoryMovementsAsync(
        DateTime? fromDate,
        DateTime? toDate,
        string? branchReference,
        string? initiatorType,
        string? status
    )
    {
        InitiatorType? parsedType = null;
        if (!string.IsNullOrEmpty(initiatorType) && Enum.TryParse<InitiatorType>(initiatorType, true, out var type))
            parsedType = type;

        InventoryTransactionStatus? parsedStatus = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<InventoryTransactionStatus>(status, true, out var stat))
            parsedStatus = stat;

        string? branchCode = null;
        string? branchName = null;
        if (!string.IsNullOrEmpty(branchReference))
        {
            var branch = await _branchRepository.GetByIdAsync(branchReference);
            branchCode = branch?.Code;
            branchName = branch?.Name;
        }

        var transactions = await _transactionRepository.SearchAsync(
            branchReference,
            fromDate,
            toDate,
            parsedType,
            parsedStatus,
            itemCode: null,
            condition: null,
            page: 1,
            pageSize: 100000
        );

        var movementTransactions = transactions.Select(t => new MovementTransactionDto
        {
            Number = t.Initiator.ReferenceNumber,
            BranchCode = t.BranchCode,
            Type = t.Initiator.EntityDefinitionCode.ToString(),
            Status = t.Status.ToString(),
            TransactionDateUtc = t.TransactionDateUtc,
            StatusHistory = t.StatusHistory.Select(sh => new StatusHistoryEntryDto
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
            Notes = t.Notes,
            Lines = t.Lines.Select(line => new MovementLineDto
            {
                ItemCode = line.ItemCode,
                Condition = line.Condition.ToString(),
                Quantity = line.Quantity,
                UnitPrice = 0m,
                Currency = Currency.USD.ToString(),
                LineTotal = 0m
            }).ToList()
        }).ToList();

        return new InventoryMovementsReportDto
        {
            GeneratedAtUtc = _clock.UtcNow,
            FromDateUtc = fromDate,
            ToDateUtc = toDate,
            BranchCode = branchCode,
            BranchName = branchName,
            TransactionType = initiatorType,
            Status = status,
            Transactions = movementTransactions.OrderBy(t => t.TransactionDateUtc).ThenBy(t => t.Number).ToList(),
            TotalCount = movementTransactions.Count()
        };
    }

    public async Task<KardexReportDto> GetKardexAsync(
        string itemCode,
        string? condition,
        string? branchReference,
        DateTime? fromDate,
        DateTime? toDate
    )
    {
        // Resolve item
        var items = await _itemRepository.GetAllAsync();
        var item = items.FirstOrDefault(i => i.ItemCode == itemCode)
            ?? throw new InvalidOperationException($"Item not found: {itemCode}");

        ItemCondition? parsedCondition = null;
        if (!string.IsNullOrEmpty(condition) && Enum.TryParse<ItemCondition>(condition, true, out var cond))
            parsedCondition = cond;

        string? branchCode = null;
        string? branchName = null;
        if (!string.IsNullOrEmpty(branchReference))
        {
            var branch = await _branchRepository.GetByIdAsync(branchReference);
            branchCode = branch?.Code;
            branchName = branch?.Name;
        }

        // Get committed transactions that involve this item
        var transactions = await _transactionRepository.SearchAsync(
            branchReference,
            fromDate,
            toDate,
            initiatorType: null,
            status: InventoryTransactionStatus.Committed,
            itemCode: itemCode,
            condition: parsedCondition,
            page: 1,
            pageSize: 100000
        );

        var orderedTransactions = transactions
            .OrderBy(t => t.TransactionDateUtc)
            .ThenBy(t => t.Initiator.ReferenceNumber)
            .ToList();

        var entries = new List<KardexEntryDto>();
        int runningBalance = 0;
        int totalIn = 0;
        int totalOut = 0;

        foreach (var txn in orderedTransactions)
        {
            var matchingLines = txn.Lines
                .Where(l => l.ItemCode == itemCode
                    && (parsedCondition == null || l.Condition == parsedCondition))
                .ToList();

            foreach (var line in matchingLines)
            {
                // PurchaseOrder and StockAdjustment are "in", Sale/StockLoss/BranchTransfer(out) are "out"
                bool isInbound = txn.Initiator.EntityDefinitionCode == InitiatorType.PurchaseOrder
                    || txn.Initiator.EntityDefinitionCode == InitiatorType.StockAdjustment;

                int inQty = isInbound ? line.Quantity : 0;
                int outQty = isInbound ? 0 : line.Quantity;
                runningBalance += inQty - outQty;
                totalIn += inQty;
                totalOut += outQty;

                entries.Add(new KardexEntryDto
                {
                    DateUtc = txn.TransactionDateUtc,
                    ReferenceNumber = txn.Initiator.ReferenceNumber,
                    Type = txn.Initiator.EntityDefinitionCode.ToString(),
                    BranchCode = txn.BranchCode,
                    In = inQty,
                    Out = outQty,
                    Balance = runningBalance,
                    Notes = txn.Notes
                });
            }
        }

        return new KardexReportDto
        {
            GeneratedAtUtc = _clock.UtcNow,
            ItemCode = itemCode,
            ItemDescription = item.Description,
            Condition = condition,
            BranchCode = branchCode,
            BranchName = branchName,
            FromDateUtc = fromDate,
            ToDateUtc = toDate,
            Entries = entries,
            TotalIn = totalIn,
            TotalOut = totalOut
        };
    }

    public async Task<SalesByVolumeReportDto> GetSalesByVolumeAsync(
        string? branchReference,
        DateTime fromUtc,
        DateTime toUtc
    )
    {
        var isAdmin = _currentUser.RoleCodes?.Contains("ADMIN") ?? false;

        string? branchRef = null;
        string? branchCode = null;
        string? branchName = null;

        if (!string.IsNullOrEmpty(branchReference) && branchReference != "ALL")
        {
            var branch = await _branchRepository.GetByIdAsync(branchReference);
            if (branch == null)
                throw new InvalidOperationException($"Branch not found: {branchReference}");

            if (!isAdmin && !(_currentUser.BranchReferences?.Contains(branchReference) ?? false))
                throw new UnauthorizedAccessException("You do not have access to this branch");

            branchRef = branch.Id;
            branchCode = branch.Code;
            branchName = branch.Name;
        }
        else if (!isAdmin)
        {
            branchRef = _currentUser.BranchReferences?.FirstOrDefault();
            if (branchRef != null)
            {
                var branch = await _branchRepository.GetByIdAsync(branchRef);
                branchCode = branch?.Code;
                branchName = branch?.Name;
            }
        }

        var sales = await _saleRepository.SearchAsync(branchRef, fromUtc, toUtc, 1, 100000);
        var committedSales = sales.Where(s => s.Status == SaleStatus.Committed).ToList();

        // Get item descriptions
        var allItems = await _itemRepository.GetAllAsync();
        var itemMap = allItems.ToDictionary(i => i.ItemCode, i => i.Description);

        // Aggregate by ItemCode + Condition
        var volumeRows = committedSales
            .SelectMany(s => s.Lines.Where(l => l.Classification == Classification.Good))
            .GroupBy(l => new { l.ItemCode, Condition = l.Condition?.ToString() ?? "N/A" })
            .Select(g => new SalesByVolumeRowDto
            {
                ItemCode = g.Key.ItemCode,
                Description = itemMap.GetValueOrDefault(g.Key.ItemCode, g.Key.ItemCode),
                Condition = g.Key.Condition,
                QuantitySold = g.Sum(l => l.Quantity),
                Revenue = g.Sum(l => l.LineTotal),
                Currency = g.First().Currency.ToString()
            })
            .OrderByDescending(r => r.QuantitySold)
            .ToList();

        return new SalesByVolumeReportDto
        {
            GeneratedAtUtc = _clock.UtcNow,
            FromDateUtc = fromUtc,
            ToDateUtc = toUtc,
            BranchCode = branchCode,
            BranchName = branchName,
            Rows = volumeRows,
            TotalQuantitySold = volumeRows.Sum(r => r.QuantitySold),
            TotalRevenue = volumeRows.Sum(r => r.Revenue)
        };
    }

    private InvoiceTotalsDto CalculateInvoiceTotals(List<InvoiceLineDto> lines)
    {
        var grandTotal = lines.Sum(l => l.LineTotal);
        var taxableTotal = lines.Where(l => l.IsTaxable).Sum(l => l.LineTotal);
        var taxableBase = taxableTotal / (1 + DEFAULT_SALES_TAX_RATE);
        var salesTaxAmount = taxableBase * DEFAULT_SALES_TAX_RATE;
        var subtotal = grandTotal - salesTaxAmount;
        var shopFeeBase = lines.Where(l => l.AppliesShopFee).Sum(l => l.LineTotal);
        var shopFeeAmount = shopFeeBase * DEFAULT_SHOP_FEE_RATE;

        return new InvoiceTotalsDto
        {
            Subtotal = subtotal,
            TaxableBase = taxableBase,
            SalesTaxRate = DEFAULT_SALES_TAX_RATE,
            SalesTaxAmount = salesTaxAmount,
            ShopFeeBase = shopFeeBase,
            ShopFeeRate = DEFAULT_SHOP_FEE_RATE,
            ShopFeeAmount = shopFeeAmount,
            GrandTotal = grandTotal
        };
    }
}
