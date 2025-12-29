using HenryTires.Inventory.Api.Services;
using HenryTires.Inventory.Application.Common;
using HenryTires.Inventory.Application.DTOs;
using HenryTires.Inventory.Application.UseCases.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HenryTires.Inventory.Api.Controllers;

[ApiController]
[Route("api/v1/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly ExcelReportGenerator _excelGenerator;
    private readonly PdfInvoiceGenerator _pdfGenerator;

    public ReportsController(
        IReportService reportService,
        ExcelReportGenerator excelGenerator,
        PdfInvoiceGenerator pdfGenerator)
    {
        _reportService = reportService;
        _excelGenerator = excelGenerator;
        _pdfGenerator = pdfGenerator;
    }

    /// <summary>
    /// Get stock report by location (branch)
    /// </summary>
    [HttpGet("stock")]
    public async Task<ActionResult<ApiResponse<StockReportDto>>> GetStockReport([FromQuery] string? branchId)
    {
        var report = await _reportService.GetStockReportAsync(branchId);
        return Ok(ApiResponse<StockReportDto>.SuccessResponse(report));
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
    /// Get invoice for a transaction
    /// </summary>
    [HttpGet("transactions/{transactionId}/invoice")]
    public async Task<ActionResult<ApiResponse<InvoiceDto>>> GetTransactionInvoice(string transactionId)
    {
        var invoice = await _reportService.GetTransactionInvoiceAsync(transactionId);
        return Ok(ApiResponse<InvoiceDto>.SuccessResponse(invoice));
    }

    /// <summary>
    /// Get inventory movements report with filters
    /// </summary>
    [HttpGet("inventory-movements")]
    public async Task<ActionResult<ApiResponse<InventoryMovementsReportDto>>> GetInventoryMovements(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] string? branchCode,
        [FromQuery] string? transactionType,
        [FromQuery] string? status
    )
    {
        var report = await _reportService.GetInventoryMovementsAsync(
            fromDate,
            toDate,
            branchCode,
            transactionType,
            status
        );
        return Ok(ApiResponse<InventoryMovementsReportDto>.SuccessResponse(report));
    }

    // ==================== EXPORT ENDPOINTS ====================

    /// <summary>
    /// Export stock report to Excel
    /// </summary>
    [HttpGet("stock/export")]
    public async Task<IActionResult> ExportStockReport([FromQuery] string? branchId)
    {
        var report = await _reportService.GetStockReportAsync(branchId);
        var excelBytes = _excelGenerator.GenerateStockReport(report);

        var fileName = string.IsNullOrEmpty(branchId)
            ? $"Stock_Report_All_{DateTime.UtcNow:yyyyMMdd}.xlsx"
            : $"Stock_Report_{report.BranchCode}_{DateTime.UtcNow:yyyyMMdd}.xlsx";

        return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    /// <summary>
    /// Export sale invoice to PDF
    /// </summary>
    [HttpGet("sales/{saleId}/invoice/pdf")]
    public async Task<IActionResult> ExportSaleInvoicePdf(string saleId)
    {
        var invoice = await _reportService.GetSaleInvoiceAsync(saleId);
        var pdfBytes = _pdfGenerator.GenerateInvoice(invoice);

        var fileName = $"Invoice_{invoice.InvoiceNumber}_{DateTime.UtcNow:yyyyMMdd}.pdf";

        return File(pdfBytes, "application/pdf", fileName);
    }

    /// <summary>
    /// Export transaction invoice to PDF
    /// </summary>
    [HttpGet("transactions/{transactionId}/invoice/pdf")]
    public async Task<IActionResult> ExportTransactionInvoicePdf(string transactionId)
    {
        var invoice = await _reportService.GetTransactionInvoiceAsync(transactionId);
        var pdfBytes = _pdfGenerator.GenerateInvoice(invoice);

        var fileName = $"Invoice_{invoice.InvoiceNumber}_{DateTime.UtcNow:yyyyMMdd}.pdf";

        return File(pdfBytes, "application/pdf", fileName);
    }

    /// <summary>
    /// Export inventory movements report to Excel
    /// </summary>
    [HttpGet("inventory-movements/export")]
    public async Task<IActionResult> ExportInventoryMovements(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] string? branchCode,
        [FromQuery] string? transactionType,
        [FromQuery] string? status
    )
    {
        var report = await _reportService.GetInventoryMovementsAsync(
            fromDate,
            toDate,
            branchCode,
            transactionType,
            status
        );
        var excelBytes = _excelGenerator.GenerateInventoryMovementsReport(report);

        var fileName = $"Inventory_Movements_{DateTime.UtcNow:yyyyMMdd}.xlsx";

        return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}
