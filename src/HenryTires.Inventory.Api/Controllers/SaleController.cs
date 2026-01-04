using HenryTires.Inventory.Application.Common;
using HenryTires.Inventory.Application.DTOs;
using HenryTires.Inventory.Application.Ports;
using HenryTires.Inventory.Application.Ports.Inbound;
using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HenryTires.Inventory.Api.Controllers;

[ApiController]
[Route("api/v1/sales")]
[Authorize]
public class SaleController : ControllerBase
{
    private readonly ISaleService _saleService;
    private readonly ICurrentUserService _currentUser;

    public SaleController(ISaleService saleService, ICurrentUserService currentUser)
    {
        _saleService = saleService;
        _currentUser = currentUser;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<SaleDto>>> CreateSale([FromBody] CreateSaleRequest request)
    {
        // StoreSeller users can only create sales for their own branch
        if (_currentUser.UserRole == Role.StoreSeller)
        {
            if (string.IsNullOrEmpty(_currentUser.BranchCode))
            {
                return StatusCode(403, ApiResponse<object>.ErrorResponse("StoreSeller must have a branch assigned"));
            }
            // Force the sale to be created for the user's branch
            request.BranchCode = _currentUser.BranchCode;
        }

        Sale sale = await _saleService.CreateSaleAsync(request);
        SaleDto dto = MapToDto(sale);
        ApiResponse<SaleDto> response = ApiResponse<SaleDto>.SuccessResponse(dto);
        return CreatedAtAction(nameof(GetSaleById), new { id = sale.Id }, response);
    }

    [HttpPost("{id}/post")]
    public async Task<ActionResult<ApiResponse<SaleDto>>> PostSale(string id)
    {
        var sale = await _saleService.PostSaleAsync(id);
        var dto = MapToDto(sale);
        return Ok(ApiResponse<SaleDto>.SuccessResponse(dto));
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
        // StoreSeller users can only view sales from their own branch
        if (_currentUser.UserRole == Role.StoreSeller)
        {
            if (string.IsNullOrEmpty(_currentUser.BranchId))
            {
                return StatusCode(403, ApiResponse<object>.ErrorResponse("StoreSeller must have a branch assigned"));
            }
            // Override branchId parameter - force to user's branch
            branchId = _currentUser.BranchId;
        }

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
    public async Task<ActionResult<ApiResponse<SaleDto>>> GetSaleById(string id)
    {
        var sale = await _saleService.GetSaleByIdAsync(id);
        if (sale == null)
        {
            return NotFound();
        }

        // StoreSeller users can only view sales from their own branch
        if (_currentUser.UserRole == Role.StoreSeller)
        {
            if (string.IsNullOrEmpty(_currentUser.BranchId))
            {
                return StatusCode(403, ApiResponse<object>.ErrorResponse("StoreSeller must have a branch assigned"));
            }

            if (sale.BranchId != _currentUser.BranchId)
            {
                return StatusCode(403, ApiResponse<object>.ErrorResponse("Access denied: Sale belongs to a different branch"));
            }
        }

        var dto = MapToDto(sale);
        return Ok(ApiResponse<SaleDto>.SuccessResponse(dto));
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
            PaymentMethod = sale.PaymentMethod,
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
