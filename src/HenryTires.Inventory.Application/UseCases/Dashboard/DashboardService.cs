using HenryTires.Inventory.Application.DTOs;
using HenryTires.Inventory.Application.Ports;
using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Domain.Enums;

namespace HenryTires.Inventory.Application.UseCases.Dashboard;

public class DashboardService
{
    private readonly IInventoryTransactionRepository _transactionRepository;
    private readonly ISaleRepository _saleRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public DashboardService(
        IInventoryTransactionRepository transactionRepository,
        ISaleRepository saleRepository,
        IBranchRepository branchRepository,
        ICurrentUser currentUser,
        IClock clock
    )
    {
        _transactionRepository = transactionRepository;
        _saleRepository = saleRepository;
        _branchRepository = branchRepository;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<DashboardDataDto> GetDashboardDataAsync(
        DateTime startDateUtc,
        DateTime endDateUtc,
        string? branchCode = null
    )
    {
        // Validate date range
        if (endDateUtc < startDateUtc)
        {
            throw new ArgumentException("End date must be after start date");
        }

        // Validate branch access
        var validatedBranchCode = ValidateBranchAccessForQuery(branchCode);

        // Get transactions using SearchAsync with pagination (get a large batch)
        var transactions = await _transactionRepository.SearchAsync(
            branchCode: validatedBranchCode,
            from: startDateUtc,
            to: endDateUtc,
            type: null,
            status: TransactionStatus.Committed,
            itemCode: null,
            condition: null,
            page: 1,
            pageSize: 10000
        );

        var filteredTransactions = transactions.ToList();

        // Convert branchCode to branchId for Sales query
        string? branchId = null;
        if (!string.IsNullOrEmpty(validatedBranchCode))
        {
            var branch = await _branchRepository.GetByCodeAsync(validatedBranchCode);
            branchId = branch?.Id;
        }

        // Get committed sales (includes both goods and services)
        var sales = await _saleRepository.SearchAsync(
            branchId: branchId,
            from: startDateUtc,
            to: endDateUtc,
            page: 1,
            pageSize: 10000
        );

        var committedSales = sales.Where(s => s.Status == TransactionStatus.Committed).ToList();

        // Get today's date range
        var todayStart = _clock.UtcNow.Date;
        var todayEnd = todayStart.AddDays(1).AddTicks(-1);

        // Calculate summary metrics (including sales)
        var summary = await CalculateSummaryAsync(
            filteredTransactions,
            committedSales,
            todayStart,
            todayEnd
        );
        summary.StartDate = startDateUtc;
        summary.EndDate = endDateUtc;

        // Calculate branch breakdown (including sales)
        var branchBreakdown = await CalculateBranchBreakdownAsync(
            filteredTransactions,
            committedSales
        );

        // Get recent activity (last 10 transactions + sales)
        var recentActivity = await GetRecentActivityAsync(filteredTransactions, committedSales);

        return new DashboardDataDto
        {
            Summary = summary,
            BranchBreakdown = branchBreakdown,
            RecentActivity = recentActivity,
        };
    }

    private Task<DashboardSummaryDto> CalculateSummaryAsync(
        List<InventoryTransaction> transactions,
        List<Sale> sales,
        DateTime todayStart,
        DateTime todayEnd
    )
    {
        var outTransactions = transactions.Where(t => t.Type == TransactionType.Out).ToList();
        var inTransactions = transactions.Where(t => t.Type == TransactionType.In).ToList();

        var salesTotal =
            sales.Sum(s => s.Lines.Sum(l => l.LineTotal))
            + outTransactions.Sum(t => t.Lines.Sum(l => l.LineTotal));
        var purchasesTotal = inTransactions.Sum(t => t.Lines.Sum(l => l.LineTotal));

        // Calculate today's totals
        var todaySales = sales
            .Where(s => s.SaleDateUtc >= todayStart && s.SaleDateUtc <= todayEnd)
            .ToList();

        var todayOutTransactions = outTransactions
            .Where(t => t.TransactionDateUtc >= todayStart && t.TransactionDateUtc <= todayEnd)
            .ToList();

        var todayPurchases = transactions
            .Where(t =>
                t.Type == TransactionType.In
                && t.TransactionDateUtc >= todayStart
                && t.TransactionDateUtc <= todayEnd
            )
            .ToList();

        var salesToday =
            todaySales.Sum(s => s.Lines.Sum(l => l.LineTotal))
            + todayOutTransactions.Sum(t => t.Lines.Sum(l => l.LineTotal));
        var purchasesToday = todayPurchases.Sum(t => t.Lines.Sum(l => l.LineTotal));

        // Determine currency (use USD as default if mixed or no transactions)
        var currency =
            sales.FirstOrDefault()?.Lines.FirstOrDefault()?.Currency
            ?? transactions.FirstOrDefault()?.Lines.FirstOrDefault()?.Currency
            ?? "USD";

        return Task.FromResult(
            new DashboardSummaryDto
            {
                SalesTotal = salesTotal,
                PurchasesTotal = purchasesTotal,
                NetTotal = salesTotal - purchasesTotal,
                SalesToday = salesToday,
                PurchasesToday = purchasesToday,
                TotalTransactions = sales.Count + inTransactions.Count + outTransactions.Count,
                SalesTransactions = sales.Count + outTransactions.Count,
                PurchaseTransactions = inTransactions.Count,
                Currency = currency,
                StartDate = DateTime.UtcNow, // Will be set by caller
                EndDate = DateTime.UtcNow, // Will be set by caller
            }
        );
    }

    private async Task<List<BranchBreakdownDto>> CalculateBranchBreakdownAsync(
        List<InventoryTransaction> transactions,
        List<Sale> sales
    )
    {
        var allBranches = await _branchRepository.GetAllAsync();
        var branchMap = allBranches.ToDictionary(b => b.Code, b => b.Name);

        // Group sales by branch
        var salesByBranch = sales.GroupBy(s => s.BranchId);
        var transactionsByBranch = transactions.GroupBy(t => t.BranchCode);

        // Get all unique branch IDs from both sales and transactions
        var allBranchIds = salesByBranch
            .Select(g => g.Key)
            .Union(transactionsByBranch.Select(g => g.Key))
            .Distinct();

        var breakdown = new List<BranchBreakdownDto>();

        foreach (var branchId in allBranchIds)
        {
            var branchSales =
                salesByBranch.FirstOrDefault(g => g.Key == branchId)?.ToList() ?? new List<Sale>();
            var branchTransactions =
                transactionsByBranch.FirstOrDefault(g => g.Key == branchId)?.ToList()
                ?? new List<InventoryTransaction>();

            var inTransactions = branchTransactions
                .Where(t => t.Type == TransactionType.In)
                .ToList();

            var outTransactions = branchTransactions
                .Where(t => t.Type == TransactionType.Out)
                .ToList();

            var salesTotal =
                branchSales.Sum(s => s.Lines.Sum(l => l.LineTotal))
                + outTransactions.Sum(t => t.Lines.Sum(l => l.LineTotal));
            var purchasesTotal = inTransactions.Sum(t => t.Lines.Sum(l => l.LineTotal));

            var currency =
                branchSales.FirstOrDefault()?.Lines.FirstOrDefault()?.Currency
                ?? branchTransactions.FirstOrDefault()?.Lines.FirstOrDefault()?.Currency
                ?? "USD";

            breakdown.Add(
                new BranchBreakdownDto
                {
                    BranchCode = branchId,
                    BranchName = branchMap.GetValueOrDefault(branchId, branchId),
                    SalesTotal = salesTotal,
                    PurchasesTotal = purchasesTotal,
                    NetTotal = salesTotal - purchasesTotal,
                    SalesTransactionCount = branchSales.Count + outTransactions.Count,
                    PurchaseTransactionCount = inTransactions.Count,
                    Currency = currency,
                }
            );
        }

        return breakdown.OrderByDescending(b => b.SalesTotal).ToList();
    }

    private async Task<List<RecentActivityItemDto>> GetRecentActivityAsync(
        List<InventoryTransaction> transactions,
        List<Sale> sales
    )
    {
        var allBranches = await _branchRepository.GetAllAsync();
        var branchMap = allBranches.ToDictionary(b => b.Code, b => b.Name);

        var activity = new List<RecentActivityItemDto>();

        // Add sales to activity
        foreach (var sale in sales)
        {
            var amount = sale.Lines.Sum(l => l.LineTotal);
            var currency = sale.Lines.FirstOrDefault()?.Currency ?? "USD";

            activity.Add(
                new RecentActivityItemDto
                {
                    Id = sale.Id,
                    TransactionNumber = sale.SaleNumber,
                    Type = "Sale",
                    Status = sale.Status.ToString(),
                    Amount = amount,
                    Currency = currency,
                    BranchCode = sale.BranchId,
                    BranchName = branchMap.GetValueOrDefault(sale.BranchId, sale.BranchId),
                    TransactionDateUtc = sale.SaleDateUtc,
                    RelativeTime = GetRelativeTime(sale.SaleDateUtc),
                }
            );
        }

        // Add purchase transactions to activity
        var purchaseTransactions = transactions.Where(t => t.Type == TransactionType.In).ToList();

        foreach (var tx in purchaseTransactions)
        {
            var amount = tx.Lines.Sum(l => l.LineTotal);
            var currency = tx.Lines.FirstOrDefault()?.Currency ?? "USD";

            activity.Add(
                new RecentActivityItemDto
                {
                    Id = tx.Id,
                    TransactionNumber = tx.TransactionNumber,
                    Type = "Purchase",
                    Status = tx.Status.ToString(),
                    Amount = amount,
                    Currency = currency,
                    BranchCode = tx.BranchCode,
                    BranchName = branchMap.GetValueOrDefault(tx.BranchCode, tx.BranchCode),
                    TransactionDateUtc = tx.TransactionDateUtc,
                    RelativeTime = GetRelativeTime(tx.TransactionDateUtc),
                }
            );
        }

        // Sort by date and take top 10
        return activity.OrderByDescending(a => a.TransactionDateUtc).Take(10).ToList();
    }

    private string GetRelativeTime(DateTime dateUtc)
    {
        var now = _clock.UtcNow;
        var diff = now - dateUtc;

        if (diff.TotalMinutes < 1)
            return "Just now";
        if (diff.TotalMinutes < 60)
            return $"{(int)diff.TotalMinutes}m ago";
        if (diff.TotalHours < 24)
            return $"{(int)diff.TotalHours}h ago";
        if (diff.TotalDays < 7)
            return $"{(int)diff.TotalDays}d ago";

        return dateUtc.ToString("MMM d, yyyy");
    }

    private string? ValidateBranchAccessForQuery(string? branchCode)
    {
        if (_currentUser.Role == Role.Admin)
        {
            return branchCode;
        }

        if (string.IsNullOrEmpty(_currentUser.BranchId))
        {
            throw new UnauthorizedAccessException("User does not have a branch assignment");
        }

        if (!string.IsNullOrEmpty(branchCode) && branchCode != _currentUser.BranchId)
        {
            throw new UnauthorizedAccessException(
                "You can only view data for your assigned branch"
            );
        }

        return _currentUser.BranchId;
    }
}
