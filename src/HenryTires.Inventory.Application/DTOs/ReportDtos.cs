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
    public required string TransactionNumber { get; set; }
    public required string BranchCode { get; set; }
    public required string Type { get; set; }
    public required string Status { get; set; }
    public required DateTime TransactionDateUtc { get; set; }
    public DateTime? CommittedAtUtc { get; set; }
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
