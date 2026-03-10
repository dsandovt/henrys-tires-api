using HenryTires.Inventory.Application.Common;
using HenryTires.Inventory.Application.DTOs;
using HenryTires.Inventory.Application.Ports.Inbound;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HenryTires.Inventory.Api.Controllers;

[ApiController]
[Route("api/v1/purchase-order")]
[Authorize]
public class PurchaseOrdersController : ControllerBase
{
    private readonly IPurchaseOrderService _purchaseOrderService;

    public PurchaseOrdersController(IPurchaseOrderService purchaseOrderService)
    {
        _purchaseOrderService = purchaseOrderService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PurchaseOrderDto>>> CreatePurchaseOrder(
        [FromBody] CreatePurchaseOrderDto request
    )
    {
        var result = await _purchaseOrderService.CreatePurchaseOrderAsync(request);
        return CreatedAtAction(
            nameof(GetPurchaseOrderById),
            new { id = result.Id },
            ApiResponse<PurchaseOrderDto>.SuccessResponse(result)
        );
    }

    [HttpPost("{id}/receive")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PurchaseOrderDto>>> ReceivePurchaseOrder(string id)
    {
        var result = await _purchaseOrderService.ReceivePurchaseOrderAsync(id);
        return Ok(ApiResponse<PurchaseOrderDto>.SuccessResponse(result));
    }

    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PurchaseOrderDto>>> CancelPurchaseOrder(string id)
    {
        var result = await _purchaseOrderService.CancelPurchaseOrderAsync(id);
        return Ok(ApiResponse<PurchaseOrderDto>.SuccessResponse(result));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PurchaseOrderDto>>> GetPurchaseOrderById(string id)
    {
        var result = await _purchaseOrderService.GetPurchaseOrderByIdAsync(id);
        return Ok(ApiResponse<PurchaseOrderDto>.SuccessResponse(result));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<PurchaseOrderDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<PurchaseOrderDto>>>> SearchPurchaseOrders(
        [FromQuery] string? branchReference = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20
    )
    {
        var result = await _purchaseOrderService.SearchPurchaseOrdersAsync(
            branchReference,
            from,
            to,
            page,
            pageSize
        );
        return Ok(ApiResponse<PaginatedResponse<PurchaseOrderDto>>.SuccessResponse(result));
    }
}
