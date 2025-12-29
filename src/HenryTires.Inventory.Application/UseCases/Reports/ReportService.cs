using HenryTires.Inventory.Application.DTOs;
using HenryTires.Inventory.Application.Ports;
using HenryTires.Inventory.Application.Ports.Outbound;
using HenryTires.Inventory.Domain.Enums;

namespace HenryTires.Inventory.Application.UseCases.Reports;

public interface IReportService
{
    Task<StockReportDto> GetStockReportAsync(string? branchId);
    Task<InvoiceDto> GetSaleInvoiceAsync(string saleId);
    Task<InvoiceDto> GetTransactionInvoiceAsync(string transactionId);
    Task<InventoryMovementsReportDto> GetInventoryMovementsAsync(
        DateTime? fromDate,
        DateTime? toDate,
        string? branchCode,
        string? transactionType,
        string? status
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
    private const decimal DEFAULT_SALES_TAX_RATE = 0.07m; // 7%
    private const decimal DEFAULT_SHOP_FEE_RATE = 0.01m; // 1%

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
        // Get branch info if specific branch requested
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

        // Get all inventory summaries for the branch (or all branches)
        // Use a large page size to get all records for the report
        var summaries = await _inventorySummaryRepository.GetByBranchAsync(
            branchCode,
            search: null,
            condition: null,
            page: 1,
            pageSize: 100000
        );

        // Get all items that are classified as Goods
        var items = await _itemRepository.GetAllAsync(Classification.Good);
        var goodItems = items.ToDictionary(i => i.ItemCode, i => i);

        // Build rows
        var rows = new List<StockReportRow>();
        foreach (var summary in summaries)
        {
            // Only include items that are classified as Goods
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

        // Calculate totals
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
        var sale = await _saleRepository.GetByIdAsync(saleId);
        if (sale == null)
            throw new InvalidOperationException($"Sale with ID {saleId} not found");

        var branch = await _branchRepository.GetByIdAsync(sale.BranchId);
        if (branch == null)
            throw new InvalidOperationException($"Branch with ID {sale.BranchId} not found");

        // Get company info from provider
        var companyInfo = _companyInfoProvider.GetCompanyInfo();

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
            InvoiceNumber = sale.SaleNumber,
            InvoiceDateUtc = sale.SaleDateUtc,
            BranchCode = branch.Code,
            BranchName = branch.Name,
            PaymentMethod = sale.PaymentMethod.ToString(),
            CustomerName = sale.CustomerName,
            CustomerNumber = null, // Not tracked in Sale entity yet
            CustomerPhone = sale.CustomerPhone,
            PONumber = null, // Not tracked in Sale entity yet
            ServiceRep = _currentUser.Username,
            Notes = sale.Notes,
            Lines = invoiceLines,
            Totals = totals,
            GeneratedAtUtc = _clock.UtcNow
        };
    }

    public async Task<InvoiceDto> GetTransactionInvoiceAsync(string transactionId)
    {
        var transaction = await _transactionRepository.GetByIdAsync(transactionId);
        if (transaction == null)
            throw new InvalidOperationException($"Transaction with ID {transactionId} not found");

        var branch = await _branchRepository.GetByCodeAsync(transaction.BranchCode);
        if (branch == null)
            throw new InvalidOperationException($"Branch with code {transaction.BranchCode} not found");

        // Get company info from provider
        var companyInfo = _companyInfoProvider.GetCompanyInfo();

        var invoiceLines = transaction.Lines.Select(line => new InvoiceLineDto
        {
            ItemCode = line.ItemCode,
            Description = line.ItemCode, // Transactions don't have description, use ItemCode
            Condition = line.Condition.ToString(),
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
            InvoiceNumber = transaction.TransactionNumber,
            InvoiceDateUtc = transaction.TransactionDateUtc,
            BranchCode = branch.Code,
            BranchName = branch.Name,
            PaymentMethod = transaction.PaymentMethod?.ToString() ?? "N/A",
            CustomerName = null,
            CustomerNumber = null,
            CustomerPhone = null,
            PONumber = null,
            ServiceRep = _currentUser.Username,
            Notes = transaction.Notes,
            Lines = invoiceLines,
            Totals = totals,
            GeneratedAtUtc = _clock.UtcNow
        };
    }

    public async Task<InventoryMovementsReportDto> GetInventoryMovementsAsync(
        DateTime? fromDate,
        DateTime? toDate,
        string? branchCode,
        string? transactionType,
        string? status
    )
    {
        // Parse enum values from strings
        TransactionType? parsedType = null;
        if (!string.IsNullOrEmpty(transactionType) && Enum.TryParse<TransactionType>(transactionType, out var type))
            parsedType = type;

        TransactionStatus? parsedStatus = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<TransactionStatus>(status, out var stat))
            parsedStatus = stat;

        // Get branch name if specific branch requested
        string? branchName = null;
        if (!string.IsNullOrEmpty(branchCode))
        {
            var branch = await _branchRepository.GetByCodeAsync(branchCode);
            branchName = branch?.Name;
        }

        // Get transactions with filters - use large page size for report
        var transactions = await _transactionRepository.SearchAsync(
            branchCode,
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
            TransactionNumber = t.TransactionNumber,
            BranchCode = t.BranchCode,
            Type = t.Type.ToString(),
            Status = t.Status.ToString(),
            TransactionDateUtc = t.TransactionDateUtc,
            CommittedAtUtc = t.CommittedAtUtc,
            Notes = t.Notes,
            Lines = t.Lines.Select(line => new MovementLineDto
            {
                ItemCode = line.ItemCode,
                Condition = line.Condition.ToString(),
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                Currency = line.Currency.ToString(),
                LineTotal = line.LineTotal
            }).ToList()
        }).ToList();

        return new InventoryMovementsReportDto
        {
            GeneratedAtUtc = _clock.UtcNow,
            FromDateUtc = fromDate,
            ToDateUtc = toDate,
            BranchCode = branchCode,
            BranchName = branchName,
            TransactionType = transactionType,
            Status = status,
            Transactions = movementTransactions.OrderBy(t => t.TransactionDateUtc).ThenBy(t => t.TransactionNumber).ToList(),
            TotalCount = movementTransactions.Count
        };
    }

    private InvoiceTotalsDto CalculateInvoiceTotals(List<InvoiceLineDto> lines)
    {
        var subtotal = lines.Sum(l => l.LineTotal);
        var taxableBase = lines.Where(l => l.IsTaxable).Sum(l => l.LineTotal);
        var shopFeeBase = lines.Where(l => l.AppliesShopFee).Sum(l => l.LineTotal);

        var salesTaxAmount = taxableBase * DEFAULT_SALES_TAX_RATE;
        var shopFeeAmount = shopFeeBase * DEFAULT_SHOP_FEE_RATE;
        var grandTotal = subtotal + salesTaxAmount + shopFeeAmount;

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
