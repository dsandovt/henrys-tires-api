using HenryTires.Inventory.Domain.Enums;

namespace HenryTires.Inventory.Application.DTOs;

public class DashboardDataDto
{
    public required DashboardSummaryDto Summary { get; set; }
    public required List<BranchBreakdownDto> BranchBreakdown { get; set; }
    public required List<RecentActivityItemDto> RecentActivity { get; set; }
}

public class DashboardSummaryDto
{
    // Range totals
    public decimal SalesTotal { get; set; }
    public decimal PurchasesTotal { get; set; }
    public decimal NetTotal { get; set; }

    // Today totals
    public decimal SalesToday { get; set; }
    public decimal PurchasesToday { get; set; }

    // Transaction counts
    public int TotalTransactions { get; set; }
    public int SalesTransactions { get; set; }
    public int PurchaseTransactions { get; set; }

    // Metadata
    public required Currency Currency { get; set; }
    public required DateTime StartDate { get; set; }
    public required DateTime EndDate { get; set; }
}

public class BranchBreakdownDto
{
    public required string BranchCode { get; set; }
    public required string BranchName { get; set; }
    public decimal SalesTotal { get; set; }
    public decimal PurchasesTotal { get; set; }
    public decimal NetTotal { get; set; }
    public int SalesTransactionCount { get; set; }
    public int PurchaseTransactionCount { get; set; }
    public required Currency Currency { get; set; }
}

public class RecentActivityItemDto
{
    public required string Id { get; set; }
    public required string TransactionNumber { get; set; }
    public required string Type { get; set; } // "Sale" or "Purchase"
    public required string Status { get; set; }
    public decimal Amount { get; set; }
    public required Currency Currency { get; set; }
    public required string BranchCode { get; set; }
    public required string BranchName { get; set; }
    public required DateTime TransactionDateUtc { get; set; }
    public required string RelativeTime { get; set; }
}
