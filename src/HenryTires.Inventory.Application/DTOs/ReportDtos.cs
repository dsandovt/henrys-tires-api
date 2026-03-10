using HenryTires.Inventory.Domain.Enums;

namespace HenryTires.Inventory.Application.DTOs;

public class StockReportDto
{
    public required DateTime GeneratedAtUtc { get; set; }
    public string? BranchCode { get; set; }
    public string? BranchName { get; set; }
    public required List<StockReportRow> Rows { get; set; }
    public required StockReportTotals Totals { get; set; }
}

public class StockReportRow
{
    public required string ItemCode { get; set; }
    public required string Description { get; set; }
    public required string Condition { get; set; }
    public required int OnHand { get; set; }
    public required int Reserved { get; set; }
    public required int Available { get; set; }
}

public class StockReportTotals
{
    public required int NewOnHand { get; set; }
    public required int NewReserved { get; set; }
    public required int NewAvailable { get; set; }
    public required int UsedOnHand { get; set; }
    public required int UsedReserved { get; set; }
    public required int UsedAvailable { get; set; }
    public required int TotalOnHand { get; set; }
    public required int TotalReserved { get; set; }
    public required int TotalAvailable { get; set; }
}

public class InvoiceDto
{
    // Company Information
    public required InvoiceCompanyInfoDto CompanyInfo { get; set; }

    // Invoice Metadata
    public required string InvoiceNumber { get; set; }
    public required DateTime InvoiceDateUtc { get; set; }
    public required string BranchCode { get; set; }
    public required string BranchName { get; set; }
    public required string PaymentMethod { get; set; }
    public List<PaymentDetailDto>? PaymentDetails { get; set; }

    // Customer Information
    public string? CustomerName { get; set; }
    public string? CustomerNumber { get; set; }
    public string? CustomerPhone { get; set; }
    public string? PONumber { get; set; }

    // Service Information
    public string? ServiceRep { get; set; }

    // Line Items and Totals
    public string? Notes { get; set; }
    public required List<InvoiceLineDto> Lines { get; set; }
    public required InvoiceTotalsDto Totals { get; set; }

    // Generation Info
    public required DateTime GeneratedAtUtc { get; set; }
    public string DocumentType { get; set; } = "INVOICE";
}

public class InvoiceCompanyInfoDto
{
    public required string LegalName { get; set; }
    public string? TradeName { get; set; }
    public required string AddressLine1 { get; set; }
    public required string CityStateZip { get; set; }
    public required string Phone { get; set; }
}

public class InvoiceLineDto
{
    public required string ItemCode { get; set; }
    public required string Description { get; set; }
    public string? Condition { get; set; }
    public required int Quantity { get; set; }
    public required decimal UnitPrice { get; set; }
    public required string Currency { get; set; }
    public required decimal LineTotal { get; set; }
    public bool IsTaxable { get; set; }
    public bool AppliesShopFee { get; set; }
}

public class InvoiceTotalsDto
{
    public required decimal Subtotal { get; set; }
    public required decimal TaxableBase { get; set; }
    public required decimal SalesTaxRate { get; set; }
    public required decimal SalesTaxAmount { get; set; }
    public required decimal ShopFeeBase { get; set; }
    public required decimal ShopFeeRate { get; set; }
    public required decimal ShopFeeAmount { get; set; }
    public decimal Discount { get; set; } = 0;
    public required decimal GrandTotal { get; set; }
    public decimal AmountPaid { get; set; } = 0;
    public decimal AmountDue { get; set; }
}

public class SalesReportDto
{
    public required DateTime GeneratedAtUtc { get; set; }
    public DateTime? FromDateUtc { get; set; }
    public DateTime? ToDateUtc { get; set; }
    public string? BranchCode { get; set; }
    public string? BranchName { get; set; }
    public required List<SalesReportRowDto> Rows { get; set; }
    public required SalesReportTotalsDto Totals { get; set; }
    public required int TotalCount { get; set; }
}

public class SalesReportRowDto
{
    public required string SaleNumber { get; set; }
    public required string BranchCode { get; set; }
    public required string BranchName { get; set; }
    public required DateTime SaleDateUtc { get; set; }
    public string? CustomerName { get; set; }
    public required int LineCount { get; set; }
    public required string LinesSummary { get; set; }
    public required string PaymentMethod { get; set; }
    public required decimal Total { get; set; }
    public required string Currency { get; set; }
    public required string Status { get; set; }
}

public class SalesReportTotalsDto
{
    public required decimal GrandTotal { get; set; }
    public required int TotalSales { get; set; }
    public required int TotalItems { get; set; }
}

public class DailyCloseReportDto
{
    public required DateTime GeneratedAtUtc { get; set; }
    public required DateTime DateUtc { get; set; }
    public string? BranchCode { get; set; }
    public string? BranchName { get; set; }
    public required string GroupBy { get; set; }
    public required DailyCloseSummaryDto Summary { get; set; }
    public required List<DailyCloseDetailDto> Details { get; set; }
    public List<DailyCloseHourGroupDto>? HourGroups { get; set; }
}

public class DailyCloseSummaryDto
{
    public required int TotalSalesCount { get; set; }
    public required decimal TotalAmount { get; set; }
    public required string Currency { get; set; }
    public required List<PaymentBreakdownDto> PaymentBreakdown { get; set; }
}

public class PaymentBreakdownDto
{
    public required string PaymentMethod { get; set; }
    public required int Count { get; set; }
    public required decimal Amount { get; set; }
}

public class DailyCloseDetailDto
{
    public required string SaleNumber { get; set; }
    public required DateTime SaleDateUtc { get; set; }
    public string? CustomerName { get; set; }
    public required int LineCount { get; set; }
    public required string PaymentMethod { get; set; }
    public required decimal Total { get; set; }
    public required string Currency { get; set; }
}

public class DailyCloseHourGroupDto
{
    public required int Hour { get; set; }
    public required string HourLabel { get; set; }
    public required List<DailyCloseDetailDto> Sales { get; set; }
    public required decimal HourTotal { get; set; }
    public required int HourCount { get; set; }
}

public class InventoryMovementsReportDto
{
    public required DateTime GeneratedAtUtc { get; set; }
    public DateTime? FromDateUtc { get; set; }
    public DateTime? ToDateUtc { get; set; }
    public string? BranchCode { get; set; }
    public string? BranchName { get; set; }
    public string? TransactionType { get; set; }
    public string? Status { get; set; }
    public required List<MovementTransactionDto> Transactions { get; set; }
    public required int TotalCount { get; set; }
}

public class MovementTransactionDto
{
    public required string Number { get; set; }
    public required string BranchCode { get; set; }
    public required string Type { get; set; }
    public required string Status { get; set; }
    public required DateTime TransactionDateUtc { get; set; }
    public required List<StatusHistoryEntryDto> StatusHistory { get; set; }
    public string? Notes { get; set; }
    public required List<MovementLineDto> Lines { get; set; }
}

public class MovementLineDto
{
    public required string ItemCode { get; set; }
    public required string Condition { get; set; }
    public required int Quantity { get; set; }
    public required decimal UnitPrice { get; set; }
    public required string Currency { get; set; }
    public required decimal LineTotal { get; set; }
}

// ==================== Kardex Report ====================

public class KardexReportDto
{
    public required DateTime GeneratedAtUtc { get; set; }
    public required string ItemCode { get; set; }
    public required string ItemDescription { get; set; }
    public string? Condition { get; set; }
    public string? BranchCode { get; set; }
    public string? BranchName { get; set; }
    public DateTime? FromDateUtc { get; set; }
    public DateTime? ToDateUtc { get; set; }
    public required List<KardexEntryDto> Entries { get; set; }
    public required int TotalIn { get; set; }
    public required int TotalOut { get; set; }
}

public class KardexEntryDto
{
    public required DateTime DateUtc { get; set; }
    public required string ReferenceNumber { get; set; }
    public required string Type { get; set; }
    public required string BranchCode { get; set; }
    public required int In { get; set; }
    public required int Out { get; set; }
    public required int Balance { get; set; }
    public string? Notes { get; set; }
}

// ==================== Sales by Volume Report ====================

public class SalesByVolumeReportDto
{
    public required DateTime GeneratedAtUtc { get; set; }
    public DateTime? FromDateUtc { get; set; }
    public DateTime? ToDateUtc { get; set; }
    public string? BranchCode { get; set; }
    public string? BranchName { get; set; }
    public required List<SalesByVolumeRowDto> Rows { get; set; }
    public required int TotalQuantitySold { get; set; }
    public required decimal TotalRevenue { get; set; }
}

public class SalesByVolumeRowDto
{
    public required string ItemCode { get; set; }
    public required string Description { get; set; }
    public required string Condition { get; set; }
    public required int QuantitySold { get; set; }
    public required decimal Revenue { get; set; }
    public required string Currency { get; set; }
}
