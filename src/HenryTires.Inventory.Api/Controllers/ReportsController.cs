using HenryTires.Inventory.Api.Helpers;
using HenryTires.Inventory.Api.Services;
using HenryTires.Inventory.Application.Common;
using HenryTires.Inventory.Application.DTOs;
using HenryTires.Inventory.Application.UseCases.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HenryTires.Inventory.Api.Controllers;

[ApiController]
[Route("api/v1/report")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly ExcelReportGenerator _excelGenerator;
    private readonly PdfInvoiceGenerator _pdfGenerator;

    public ReportsController(
        IReportService reportService,
        ExcelReportGenerator excelGenerator,
        PdfInvoiceGenerator pdfGenerator
    )
    {
        _reportService = reportService;
        _excelGenerator = excelGenerator;
        _pdfGenerator = pdfGenerator;
    }

    /// <summary>
    /// Get stock report by location (branch)
    /// </summary>
    [HttpGet("stock")]
    public async Task<ActionResult<ApiResponse<StockReportDto>>> GetStockReport(
        [FromQuery] string? branchId
    )
    {
        var report = await _reportService.GetStockReportAsync(branchId);
        return Ok(ApiResponse<StockReportDto>.SuccessResponse(report));
    }

    /// <summary>
    /// Get sales report by branch and date range
    /// </summary>
    [HttpGet("sales")]
    public async Task<ActionResult<ApiResponse<SalesReportDto>>> GetSalesReport(
        [FromQuery] string? branchReference,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] string sortOrder = "desc",
        [FromQuery] int timezoneOffset = -300
    )
    {
        var fromUtc = TimezoneHelper.LocalStartOfDayToUtc(from, timezoneOffset);
        var toUtc = TimezoneHelper.LocalEndOfDayToUtc(to, timezoneOffset);

        var report = await _reportService.GetSalesReportAsync(branchReference, fromUtc, toUtc, sortOrder);
        return Ok(ApiResponse<SalesReportDto>.SuccessResponse(report));
    }

    /// <summary>
    /// Get invoice for a sale
    /// </summary>
    [HttpGet("sales/{saleId}/invoice")]
    public async Task<ActionResult<ApiResponse<InvoiceDto>>> GetSaleInvoice(string saleId)
    {
        var invoice = await _reportService.GetSaleInvoiceAsync(saleId);
        return Ok(ApiResponse<InvoiceDto>.SuccessResponse(invoice));
    }

    /// <summary>
    /// Get inventory movements report with filters
    /// </summary>
    [HttpGet("inventory-movements")]
    public async Task<ActionResult<ApiResponse<InventoryMovementsReportDto>>> GetInventoryMovements(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] string? branchReference,
        [FromQuery] string? initiatorType,
        [FromQuery] string? status
    )
    {
        var report = await _reportService.GetInventoryMovementsAsync(
            fromDate,
            toDate,
            branchReference,
            initiatorType,
            status
        );
        return Ok(ApiResponse<InventoryMovementsReportDto>.SuccessResponse(report));
    }

    /// <summary>
    /// Get daily close report for a branch and date
    /// </summary>
    [HttpGet("daily-close")]
    public async Task<ActionResult<ApiResponse<DailyCloseReportDto>>> GetDailyCloseReport(
        [FromQuery] string branchReference,
        [FromQuery] DateTime date,
        [FromQuery] string groupBy = "day",
        [FromQuery] int timezoneOffset = -300
    )
    {
        var report = await _reportService.GetDailyCloseReportAsync(branchReference, date, groupBy, timezoneOffset);
        return Ok(ApiResponse<DailyCloseReportDto>.SuccessResponse(report));
    }

    /// <summary>
    /// Get kardex (product ledger) report
    /// </summary>
    [HttpGet("kardex")]
    public async Task<ActionResult<ApiResponse<KardexReportDto>>> GetKardex(
        [FromQuery] string itemCode,
        [FromQuery] string? condition,
        [FromQuery] string? branchReference,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate
    )
    {
        var report = await _reportService.GetKardexAsync(
            itemCode,
            condition,
            branchReference,
            fromDate,
            toDate
        );
        return Ok(ApiResponse<KardexReportDto>.SuccessResponse(report));
    }

    /// <summary>
    /// Get sales by volume report
    /// </summary>
    [HttpGet("sales-by-volume")]
    public async Task<ActionResult<ApiResponse<SalesByVolumeReportDto>>> GetSalesByVolume(
        [FromQuery] string? branchReference,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] int timezoneOffset = -300
    )
    {
        var fromUtc = TimezoneHelper.LocalStartOfDayToUtc(from, timezoneOffset);
        var toUtc = TimezoneHelper.LocalEndOfDayToUtc(to, timezoneOffset);

        var report = await _reportService.GetSalesByVolumeAsync(branchReference, fromUtc, toUtc);
        return Ok(ApiResponse<SalesByVolumeReportDto>.SuccessResponse(report));
    }

    // ==================== EXPORT ENDPOINTS ====================

    /// <summary>
    /// Export stock report to Excel
    /// </summary>
    [HttpGet("stock/export")]
    public async Task<IActionResult> ExportStockReport(
        [FromQuery] string? branchId,
        [FromQuery] int timezoneOffset = -300
    )
    {
        var report = await _reportService.GetStockReportAsync(branchId);
        var excelBytes = _excelGenerator.GenerateStockReport(report, timezoneOffset);

        var fileName = string.IsNullOrEmpty(branchId)
            ? $"Stock_Report_All_{DateTime.UtcNow:yyyyMMdd}.xlsx"
            : $"Stock_Report_{report.BranchCode}_{DateTime.UtcNow:yyyyMMdd}.xlsx";

        return File(
            excelBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName
        );
    }

    /// <summary>
    /// Export sales report to Excel
    /// </summary>
    [HttpGet("sales/export")]
    public async Task<IActionResult> ExportSalesReport(
        [FromQuery] string? branchReference,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] string sortOrder = "desc",
        [FromQuery] int timezoneOffset = -300
    )
    {
        var fromUtc = TimezoneHelper.LocalStartOfDayToUtc(from, timezoneOffset);
        var toUtc = TimezoneHelper.LocalEndOfDayToUtc(to, timezoneOffset);

        var report = await _reportService.GetSalesReportAsync(branchReference, fromUtc, toUtc, sortOrder);
        var excelBytes = _excelGenerator.GenerateSalesReport(report, timezoneOffset);

        var fileName = $"Sales_Report_{DateTime.UtcNow:yyyyMMdd}.xlsx";
        return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    /// <summary>
    /// Export daily close report to Excel
    /// </summary>
    [HttpGet("daily-close/export")]
    public async Task<IActionResult> ExportDailyCloseReport(
        [FromQuery] string branchReference,
        [FromQuery] DateTime date,
        [FromQuery] string groupBy = "day",
        [FromQuery] int timezoneOffset = -300
    )
    {
        var report = await _reportService.GetDailyCloseReportAsync(branchReference, date, groupBy, timezoneOffset);
        var excelBytes = _excelGenerator.GenerateDailyCloseReport(report, timezoneOffset);

        var fileName = $"Daily_Close_{report.BranchCode ?? "All"}_{date:yyyyMMdd}.xlsx";
        return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    /// <summary>
    /// Export sale invoice to PDF
    /// </summary>
    [HttpGet("sales/{saleId}/invoice/pdf")]
    public async Task<IActionResult> ExportSaleInvoicePdf(
        string saleId,
        [FromQuery] int timezoneOffset = -300
    )
    {
        var invoice = await _reportService.GetSaleInvoiceAsync(saleId);
        var pdfBytes = _pdfGenerator.GenerateInvoice(invoice, timezoneOffset);

        var fileName = $"Invoice_{invoice.InvoiceNumber}_{DateTime.UtcNow:yyyyMMdd}.pdf";

        return File(pdfBytes, "application/pdf", fileName);
    }

    /// <summary>
    /// Export kardex report to Excel
    /// </summary>
    [HttpGet("kardex/export")]
    public async Task<IActionResult> ExportKardex(
        [FromQuery] string itemCode,
        [FromQuery] string? condition,
        [FromQuery] string? branchReference,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int timezoneOffset = -300
    )
    {
        var report = await _reportService.GetKardexAsync(itemCode, condition, branchReference, fromDate, toDate);
        var excelBytes = _excelGenerator.GenerateKardexReport(report, timezoneOffset);

        var fileName = $"Kardex_{itemCode}_{DateTime.UtcNow:yyyyMMdd}.xlsx";
        return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    /// <summary>
    /// Export sales by volume report to Excel
    /// </summary>
    [HttpGet("sales-by-volume/export")]
    public async Task<IActionResult> ExportSalesByVolume(
        [FromQuery] string? branchReference,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] int timezoneOffset = -300
    )
    {
        var fromUtc = TimezoneHelper.LocalStartOfDayToUtc(from, timezoneOffset);
        var toUtc = TimezoneHelper.LocalEndOfDayToUtc(to, timezoneOffset);

        var report = await _reportService.GetSalesByVolumeAsync(branchReference, fromUtc, toUtc);
        var excelBytes = _excelGenerator.GenerateSalesByVolumeReport(report, timezoneOffset);

        var fileName = $"Sales_By_Volume_{DateTime.UtcNow:yyyyMMdd}.xlsx";
        return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    /// <summary>
    /// Export inventory movements report to Excel
    /// </summary>
    [HttpGet("inventory-movements/export")]
    public async Task<IActionResult> ExportInventoryMovements(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] string? branchReference,
        [FromQuery] string? initiatorType,
        [FromQuery] string? status,
        [FromQuery] int timezoneOffset = -300
    )
    {
        var report = await _reportService.GetInventoryMovementsAsync(
            fromDate,
            toDate,
            branchReference,
            initiatorType,
            status
        );
        var excelBytes = _excelGenerator.GenerateInventoryMovementsReport(report, timezoneOffset);

        var fileName = $"Inventory_Movements_{DateTime.UtcNow:yyyyMMdd}.xlsx";

        return File(
            excelBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName
        );
    }
}
