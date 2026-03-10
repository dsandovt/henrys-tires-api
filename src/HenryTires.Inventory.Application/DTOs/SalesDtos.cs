using HenryTires.Inventory.Domain.Enums;

namespace HenryTires.Inventory.Application.DTOs;

public class SaleDto
{
    public required string Id { get; set; }
    public required string Number { get; set; }
    public required string BranchReference { get; set; }
    public required string BranchCode { get; set; }
    public required DateTime SaleDateUtc { get; set; }
    public required List<SaleLineDto> Lines { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? Notes { get; set; }
    public required PaymentMethod PaymentMethod { get; set; }
    public List<PaymentDetailDto>? PaymentDetails { get; set; }
    public required SaleStatus Status { get; set; }
    public required List<StatusHistoryEntryDto> StatusHistory { get; set; }
    public required string CreatedBy { get; set; }
    public required DateTime CreatedAtUtc { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }
    public string? ModifiedBy { get; set; }
}

public class SaleLineDto
{
    public required string LineId { get; set; }
    public required string ItemReference { get; set; }
    public required string ItemCode { get; set; }
    public required string Description { get; set; }
    public required Classification Classification { get; set; }
    public ItemCondition? Condition { get; set; }
    public required int Quantity { get; set; }
    public required decimal UnitPrice { get; set; }
    public required Currency Currency { get; set; }
    public bool IsTaxable { get; set; } = true;
    public bool AppliesShopFee { get; set; } = true;
    public required decimal LineTotal { get; set; }
}

public class PaymentDetailDto
{
    public required string Method { get; set; }
    public required decimal Amount { get; set; }
    public string? CheckNumber { get; set; }
}

public class CreateSaleRequest
{
    public string? BranchCode { get; set; }
    public DateTime? SaleDateUtc { get; set; }
    public required List<CreateSaleLineRequest> Lines { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? Notes { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    public List<PaymentDetailDto>? PaymentDetails { get; set; }
}

public class CreateSaleLineRequest
{
    public required string ItemReference { get; set; }
    public required string ItemCode { get; set; }
    public required string Description { get; set; }
    public required Classification Classification { get; set; }
    public ItemCondition? Condition { get; set; }
    public required int Quantity { get; set; }
    public required decimal UnitPrice { get; set; }
    public required Currency Currency { get; set; }
    public bool IsTaxable { get; set; } = true;
    public bool AppliesShopFee { get; set; } = true;
    public bool AllowWithoutStock { get; set; } = false;
}

public class StatusHistoryEntryDto
{
    public required DateTime Date { get; set; }
    public required string Status { get; set; }
    public required UserLiteDto User { get; set; }
    public string? Comment { get; set; }
}

public class UserLiteDto
{
    public required string FirstName { get; set; }
    public string? MiddleName { get; set; }
    public required string LastName { get; set; }
    public string? SecondLastName { get; set; }
    public required string Username { get; set; }
    public string? Email { get; set; }
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
    public required string BranchReference { get; set; }
    public required string BranchCode { get; set; }
    public required decimal TotalAmount { get; set; }
    public required int NewTiresCount { get; set; }
    public required int UsedTiresCount { get; set; }
    public required string StatusColor { get; set; }
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