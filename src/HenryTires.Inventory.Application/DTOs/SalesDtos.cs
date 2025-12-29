using HenryTires.Inventory.Domain.Enums;

namespace HenryTires.Inventory.Application.DTOs;

public class SaleDto
{
    public required string Id { get; set; }
    public required string SaleNumber { get; set; }
    public required string BranchId { get; set; }
    public required DateTime SaleDateUtc { get; set; }
    public required List<SaleLineDto> Lines { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? Notes { get; set; }
    public required TransactionStatus Status { get; set; }
    public DateTime? PostedAtUtc { get; set; }
    public string? PostedBy { get; set; }
    public required string CreatedBy { get; set; }
    public required DateTime CreatedAtUtc { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }
    public string? ModifiedBy { get; set; }
}

public class SaleLineDto
{
    public required string LineId { get; set; }
    public required string ItemId { get; set; }
    public required string ItemCode { get; set; }
    public required string Description { get; set; }
    public required Classification Classification { get; set; } // Good or Service
    public ItemCondition? Condition { get; set; } // Required for Goods, null for Services
    public required int Quantity { get; set; }
    public required decimal UnitPrice { get; set; }
    public required Currency Currency { get; set; }
    public required decimal LineTotal { get; set; }
    public string? InventoryTransactionId { get; set; } // Set only for Goods after posting
}

public class CreateSaleRequest
{
    public string? BranchId { get; set; } // Required for Admin users
    public required DateTime SaleDateUtc { get; set; }
    public required List<CreateSaleLineRequest> Lines { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? Notes { get; set; }
}

public class CreateSaleLineRequest
{
    public required string ItemId { get; set; }
    public required string ItemCode { get; set; }
    public required string Description { get; set; }
    public required Classification Classification { get; set; }
    public ItemCondition? Condition { get; set; } // Required for Goods, null for Services
    public required int Quantity { get; set; }
    public required decimal UnitPrice { get; set; }
    public required Currency Currency { get; set; }
}

public class SalesDashboardDto
{
    public required List<SalesDayRowDto> Days { get; set; }
    public required SalesTotalDto Totals { get; set; }
    public required SalesRevenueDto Revenue { get; set; }
}

public class SalesDayRowDto
{
    public required DateTime Date { get; set; }
    public required Dictionary<string, BranchSalesDto> BranchSales { get; set; }
    public required decimal DayTotal { get; set; }
    public required int DayNewTires { get; set; }
    public required int DayUsedTires { get; set; }
}

public class BranchSalesDto
{
    public required string BranchId { get; set; }
    public required string BranchCode { get; set; }
    public required decimal TotalAmount { get; set; }
    public required int NewTiresCount { get; set; }
    public required int UsedTiresCount { get; set; }
    public required string StatusColor { get; set; } // "green", "red", "orange"
}

public class SalesTotalDto
{
    public required Dictionary<string, BranchTotalDto> ByBranch { get; set; }
    public required decimal GrandTotal { get; set; }
    public required int TotalNewTires { get; set; }
    public required int TotalUsedTires { get; set; }
}

public class BranchTotalDto
{
    public required string BranchCode { get; set; }
    public required decimal TotalAmount { get; set; }
    public required int NewTiresCount { get; set; }
    public required int UsedTiresCount { get; set; }
}

public class SalesRevenueDto
{
    public required decimal Daily { get; set; }
    public required decimal Weekly { get; set; }
    public required decimal Monthly { get; set; }
    public required decimal DailyPurchases { get; set; }
    public required decimal WeeklyPurchases { get; set; }
    public required decimal MonthlyPurchases { get; set; }
}

public class SalesListResponse
{
    public required IEnumerable<SaleDto> Items { get; set; }
    public required int TotalCount { get; set; }
    public required int Page { get; set; }
    public required int PageSize { get; set; }
}
