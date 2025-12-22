using HenryTires.Inventory.Application.DTOs;
using HenryTires.Inventory.Application.UseCases.Sales;
using HenryTires.Inventory.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace HenryTires.Inventory.Api.Controllers;

[ApiController]
[Route("api/v1/sales")]
[Authorize]
public class SalesController : ControllerBase
{
    private readonly SaleService _saleService;

    public SalesController(SaleService saleService)
    {
        _saleService = saleService;
    }

    /// <summary>
    /// Create a new Sale (Draft status).
    /// Can include BOTH Goods and Services.
    /// Does NOT post to inventory yet.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<SaleDto>> CreateSale([FromBody] CreateSaleRequest request)
    {
        var lines = request.Lines.Select(l => new SaleLine
        {
            LineId = ObjectId.GenerateNewId().ToString(),
            ItemId = l.ItemId,
            ItemCode = l.ItemCode,
            Description = l.Description,
            Classification = l.Classification,
            Condition = l.Condition,
            Quantity = l.Quantity,
            UnitPrice = l.UnitPrice,
            Currency = l.Currency
        }).ToList();

        var sale = await _saleService.CreateSaleAsync(
            request.BranchId,
            request.SaleDateUtc,
            lines,
            request.CustomerName,
            request.CustomerPhone,
            request.Notes
        );

        return Ok(MapToDto(sale));
    }

    /// <summary>
    /// Post Sale to inventory.
    /// - Generates InventoryTransaction (OUT) for Goods ONLY
    /// - Services are ignored (revenue only)
    /// - Uses MongoDB transaction for atomicity
    /// </summary>
    [HttpPost("{id}/post")]
    public async Task<ActionResult<SaleDto>> PostSale(string id)
    {
        var sale = await _saleService.PostSaleAsync(id);
        return Ok(MapToDto(sale));
    }

    /// <summary>
    /// Get all Sales with optional filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<SalesListResponse>> GetSales(
        [FromQuery] string? branchId = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100)
    {
        IEnumerable<Sale> sales;

        if (!string.IsNullOrEmpty(branchId) && from.HasValue && to.HasValue)
        {
            sales = await _saleService.GetSalesByBranchAndDateRangeAsync(branchId, from.Value, to.Value);
        }
        else if (from.HasValue && to.HasValue)
        {
            sales = await _saleService.GetSalesByDateRangeAsync(from.Value, to.Value);
        }
        else
        {
            sales = await _saleService.SearchSalesAsync(branchId, from, to, page, pageSize);
        }

        var totalCount = await _saleService.CountSalesAsync(branchId, from, to);

        return Ok(new SalesListResponse
        {
            Items = sales.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    /// <summary>
    /// Get Sale by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<SaleDto>> GetSaleById(string id)
    {
        var sale = await _saleService.GetSaleByIdAsync(id);
        if (sale == null)
        {
            return NotFound();
        }

        return Ok(MapToDto(sale));
    }

    // ========================================================================
    // Helper Methods
    // ========================================================================

    private static SaleDto MapToDto(Sale sale)
    {
        return new SaleDto
        {
            Id = sale.Id,
            SaleNumber = sale.SaleNumber,
            BranchId = sale.BranchId,
            SaleDateUtc = sale.SaleDateUtc,
            Lines = sale.Lines.Select(l => new SaleLineDto
            {
                LineId = l.LineId ?? ObjectId.GenerateNewId().ToString(),
                ItemId = l.ItemId,
                ItemCode = l.ItemCode,
                Description = l.Description,
                Classification = l.Classification,
                Condition = l.Condition,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                Currency = l.Currency,
                LineTotal = l.LineTotal,
                InventoryTransactionId = l.InventoryTransactionId
            }).ToList(),
            CustomerName = sale.CustomerName,
            CustomerPhone = sale.CustomerPhone,
            Notes = sale.Notes,
            Status = sale.Status,
            PostedAtUtc = sale.PostedAtUtc,
            PostedBy = sale.PostedBy,
            CreatedAtUtc = sale.CreatedAtUtc,
            CreatedBy = sale.CreatedBy,
            ModifiedAtUtc = sale.ModifiedAtUtc,
            ModifiedBy = sale.ModifiedBy
        };
    }
}
