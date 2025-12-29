using HenryTires.Inventory.Application.Common;
using HenryTires.Inventory.Application.DTOs;
using HenryTires.Inventory.Application.Ports.Inbound;
using HenryTires.Inventory.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HenryTires.Inventory.Api.Controllers;

[ApiController]
[Route("api/v1/sales")]
[Authorize]
public class SaleController : ControllerBase
{
    private readonly ISaleService _saleService;

    public SaleController(ISaleService saleService)
    {
        _saleService = saleService;
    }

    [HttpPost]
    public async Task<ActionResult<SaleDto>> CreateSale([FromBody] CreateSaleRequest request)
    {
        var sale = await _saleService.CreateSaleAsync(request);
        var dto = MapToDto(sale);
        return CreatedAtAction(nameof(GetSaleById), new { id = sale.Id }, dto);
    }

    [HttpPost("{id}/post")]
    public async Task<ActionResult<SaleDto>> PostSale(string id)
    {
        var sale = await _saleService.PostSaleAsync(id);
        return Ok(MapToDto(sale));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<SalesListResponse>>> GetSales(
        [FromQuery] string? branchId = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100
    )
    {
        IEnumerable<Sale> sales;

        if (!string.IsNullOrEmpty(branchId) && from.HasValue && to.HasValue)
        {
            sales = await _saleService.GetSalesByBranchAndDateRangeAsync(
                branchId,
                from.Value,
                to.Value
            );
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

        var response = new SalesListResponse
        {
            Items = sales.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };

        return Ok(ApiResponse<SalesListResponse>.SuccessResponse(response));
    }

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

    private static SaleDto MapToDto(Sale sale)
    {
        return new SaleDto
        {
            Id = sale.Id,
            SaleNumber = sale.SaleNumber,
            BranchId = sale.BranchId,
            SaleDateUtc = sale.SaleDateUtc,
            Lines = sale
                .Lines.Select(l => new SaleLineDto
                {
                    LineId = l.LineId!,
                    ItemId = l.ItemId,
                    ItemCode = l.ItemCode,
                    Description = l.Description,
                    Classification = l.Classification,
                    Condition = l.Condition,
                    Quantity = l.Quantity,
                    UnitPrice = l.UnitPrice,
                    Currency = l.Currency,
                    LineTotal = l.LineTotal,
                    InventoryTransactionId = l.InventoryTransactionId,
                })
                .ToList(),
            CustomerName = sale.CustomerName,
            CustomerPhone = sale.CustomerPhone,
            Notes = sale.Notes,
            Status = sale.Status,
            PostedAtUtc = sale.PostedAtUtc,
            PostedBy = sale.PostedBy,
            CreatedAtUtc = sale.CreatedAtUtc,
            CreatedBy = sale.CreatedBy,
            ModifiedAtUtc = sale.ModifiedAtUtc,
            ModifiedBy = sale.ModifiedBy,
        };
    }
}
