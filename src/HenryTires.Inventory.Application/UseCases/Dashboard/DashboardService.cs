using HenryTires.Inventory.Application.DTOs;
using HenryTires.Inventory.Application.Ports;
using HenryTires.Inventory.Application.Ports.Inbound;
using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Domain.Enums;

namespace HenryTires.Inventory.Application.UseCases.Dashboard;

public class DashboardService : IDashboardService
{
    private readonly ISaleRepository _saleRepository;
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public DashboardService(
        ISaleRepository saleRepository,
        IPurchaseOrderRepository purchaseOrderRepository,
        IBranchRepository branchRepository,
        ICurrentUser currentUser,
        IClock clock
    )
    {
        _saleRepository = saleRepository;
        _purchaseOrderRepository = purchaseOrderRepository;
        _branchRepository = branchRepository;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<DashboardDataDto> GetDashboardDataAsync(
        DateTime startDateUtc,
        DateTime endDateUtc,
        string? branchReference = null
    )
    {
        if (endDateUtc < startDateUtc)
            throw new ArgumentException("End date must be after start date");

        var validatedBranchReference = ValidateBranchAccessForQuery(branchReference);

        var sales = await _saleRepository.SearchAsync(
            branchReference: validatedBranchReference,
            from: startDateUtc,
            to: endDateUtc,
            page: 1,
            pageSize: 10000
        );

        var postedSales = sales.Where(s => s.Status == SaleStatus.Committed).ToList();

        var purchaseOrders = await _purchaseOrderRepository.SearchAsync(
            branchReference: validatedBranchReference,
            from: startDateUtc,
            to: endDateUtc,
            page: 1,
            pageSize: 10000
        );

        var receivedPurchaseOrders = purchaseOrders
            .Where(po => po.Status == PurchaseOrderStatus.Received)
            .ToList();

        var todayStart = _clock.UtcNow.Date;
        var todayEnd = todayStart.AddDays(1).AddTicks(-1);

        var summary = CalculateSummary(postedSales, receivedPurchaseOrders, todayStart, todayEnd);
        summary.StartDate = startDateUtc;
        summary.EndDate = endDateUtc;

        var branchBreakdown = await CalculateBranchBreakdownAsync(postedSales, receivedPurchaseOrders);

        var recentActivity = await GetRecentActivityAsync(postedSales, receivedPurchaseOrders);

        return new DashboardDataDto
        {
            Summary = summary,
            BranchBreakdown = branchBreakdown,
            RecentActivity = recentActivity,
        };
    }

    private DashboardSummaryDto CalculateSummary(
        List<Sale> sales,
        List<PurchaseOrder> purchaseOrders,
        DateTime todayStart,
        DateTime todayEnd
    )
    {
        var salesTotal = sales.SelectMany(s => s.Lines).Sum(l => l.LineTotal);
        var purchasesTotal = purchaseOrders.SelectMany(po => po.Lines).Sum(l => l.LineTotal);

        var salesToday = sales
            .Where(s => s.SaleDateUtc >= todayStart && s.SaleDateUtc <= todayEnd)
            .SelectMany(s => s.Lines)
            .Sum(l => l.LineTotal);

        var purchasesToday = purchaseOrders
            .Where(po => po.OrderDateUtc >= todayStart && po.OrderDateUtc <= todayEnd)
            .SelectMany(po => po.Lines)
            .Sum(l => l.LineTotal);

        var currency = sales.FirstOrDefault()?.Lines.FirstOrDefault()?.Currency
            ?? purchaseOrders.FirstOrDefault()?.Lines.FirstOrDefault()?.Currency
            ?? Currency.USD;

        return new DashboardSummaryDto
        {
            SalesTotal = salesTotal,
            PurchasesTotal = purchasesTotal,
            NetTotal = salesTotal - purchasesTotal,
            SalesToday = salesToday,
            PurchasesToday = purchasesToday,
            TotalTransactions = sales.Count + purchaseOrders.Count,
            SalesTransactions = sales.Count,
            PurchaseTransactions = purchaseOrders.Count,
            Currency = currency,
            StartDate = _clock.UtcNow,
            EndDate = _clock.UtcNow,
        };
    }

    private async Task<List<BranchBreakdownDto>> CalculateBranchBreakdownAsync(
        List<Sale> sales,
        List<PurchaseOrder> purchaseOrders
    )
    {
        var allBranches = await _branchRepository.GetAllAsync();
        var branchMap = allBranches.ToDictionary(b => b.Code, b => b.Name);

        var salesByBranch = sales.Where(s => s.BranchCode != null).GroupBy(s => s.BranchCode!);
        var posByBranch = purchaseOrders.Where(po => po.BranchCode != null).GroupBy(po => po.BranchCode!);

        var allBranchCodes = salesByBranch.Select(g => g.Key)
            .Union(posByBranch.Select(g => g.Key))
            .Distinct();

        var breakdown = new List<BranchBreakdownDto>();

        foreach (var branchCode in allBranchCodes)
        {
            var branchSales =
                salesByBranch.FirstOrDefault(g => g.Key == branchCode)?.ToList() ?? new List<Sale>();
            var branchPOs =
                posByBranch.FirstOrDefault(g => g.Key == branchCode)?.ToList() ?? new List<PurchaseOrder>();

            var salesTotal = branchSales.SelectMany(s => s.Lines).Sum(l => l.LineTotal);
            var purchasesTotal = branchPOs.SelectMany(po => po.Lines).Sum(l => l.LineTotal);

            var currency =
                branchSales.FirstOrDefault()?.Lines.FirstOrDefault()?.Currency
                ?? branchPOs.FirstOrDefault()?.Lines.FirstOrDefault()?.Currency
                ?? Currency.USD;

            breakdown.Add(
                new BranchBreakdownDto
                {
                    BranchCode = branchCode,
                    BranchName = branchMap.GetValueOrDefault(branchCode, branchCode),
                    SalesTotal = salesTotal,
                    PurchasesTotal = purchasesTotal,
                    NetTotal = salesTotal - purchasesTotal,
                    SalesTransactionCount = branchSales.Count,
                    PurchaseTransactionCount = branchPOs.Count,
                    Currency = currency,
                }
            );
        }

        return breakdown.OrderByDescending(b => b.SalesTotal).ToList();
    }

    private async Task<List<RecentActivityItemDto>> GetRecentActivityAsync(
        List<Sale> sales,
        List<PurchaseOrder> purchaseOrders
    )
    {
        var allBranches = await _branchRepository.GetAllAsync();
        var branchMap = allBranches.ToDictionary(b => b.Code, b => b.Name);

        var activity = new List<RecentActivityItemDto>();

        foreach (var sale in sales)
        {
            var amount = sale.Lines.Sum(l => l.LineTotal);
            var currency = sale.Lines.FirstOrDefault()?.Currency ?? Currency.USD;

            activity.Add(
                new RecentActivityItemDto
                {
                    Id = sale.Id,
                    Number = sale.Number,
                    Type = "Sale",
                    Status = sale.Status.ToString(),
                    Amount = amount,
                    Currency = currency,
                    BranchCode = sale.BranchCode,
                    BranchName = branchMap.GetValueOrDefault(sale.BranchCode, sale.BranchCode),
                    TransactionDateUtc = sale.SaleDateUtc,
                    RelativeTime = GetRelativeTime(sale.SaleDateUtc),
                }
            );
        }

        foreach (var po in purchaseOrders)
        {
            var amount = po.Lines.Sum(l => l.LineTotal);
            var currency = po.Lines.FirstOrDefault()?.Currency ?? Currency.USD;

            activity.Add(
                new RecentActivityItemDto
                {
                    Id = po.Id,
                    Number = po.Number,
                    Type = "Purchase",
                    Status = po.Status.ToString(),
                    Amount = amount,
                    Currency = currency,
                    BranchCode = po.BranchCode,
                    BranchName = branchMap.GetValueOrDefault(po.BranchCode, po.BranchCode),
                    TransactionDateUtc = po.OrderDateUtc,
                    RelativeTime = GetRelativeTime(po.OrderDateUtc),
                }
            );
        }

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

    private string? ValidateBranchAccessForQuery(string? branchReference)
    {
        if (_currentUser.HasRole("ADMIN"))
            return branchReference;

        if (_currentUser.BranchReferences.Count == 0)
            throw new UnauthorizedAccessException("User does not have a branch assignment");

        if (!string.IsNullOrEmpty(branchReference))
        {
            if (!_currentUser.CanAccessBranch(branchReference))
                throw new UnauthorizedAccessException(
                    "You can only view data for your assigned branches"
                );
            return branchReference;
        }

        return _currentUser.BranchReferences[0];
    }
}
